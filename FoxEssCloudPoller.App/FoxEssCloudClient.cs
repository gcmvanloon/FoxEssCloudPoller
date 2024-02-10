using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace FoxEssCloudPoller
{
    public class FoxEssCloudClient
    {
        private string _username;
        private string _hashedPassword;
        private string _inverterId;
        private string _token;
        private ILogger<FoxEssCloudClient> _logger;

        public FoxEssCloudClient(IConfiguration configuration, ILogger<FoxEssCloudClient> logger)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            _username = configuration.GetValue<string>("FoxEssCloud:User");
            _inverterId = configuration.GetValue<string>("FoxEssCloud:InverterId");

            var password = configuration.GetValue<string>("FoxEssCloud:Password");
            _hashedPassword = BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(password)))
                .Replace("-", string.Empty)
                .ToLower();

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<RawResponse> GetHourlyRawValuesAsync(DateTime offset)
        {
            try
            {
                return await GetHourlyRawValuesInternalAsync(offset);
            }
            catch (TokenExpiredException)
            {
                //renew token
                await GetTokenAsync();
            }
            //try again with the new token
            return await GetHourlyRawValuesInternalAsync(offset);
        }

        private async Task<RawResponse> GetHourlyRawValuesInternalAsync(DateTime offset)
        {
            if (_token == null)
            {
                _logger.LogDebug("First time to access FoxEssCloud, need to aquire token first.");
                await GetTokenAsync();
            }
            using (HttpClient client = new HttpClient())
            {
                var content = JsonContent.Create(new
                {
                    deviceID = _inverterId,
                    variables = new string[] {
                        FoxEssVariables.GenerationPower,
                        FoxEssVariables.InvTemperation,
                        FoxEssVariables.PV1Volt,
                        FoxEssVariables.PV2Volt,
                        FoxEssVariables.PV3Volt,
                        FoxEssVariables.PV4Volt
                    },
                    timespan = "hour",
                    begindate = new
                    {
                        year = offset.Year,
                        month = offset.Month,
                        day = offset.Day,
                        hour = offset.Hour
                    }
                });
                content.Headers.Add("token", _token);
                _logger.LogDebug("Posting request to foxesscloud.com for raw measurements...");
                var response = await client.PostAsync("https://www.foxesscloud.com/c/v0/device/history/raw", content);

                if (response.IsSuccessStatusCode)
                {
                    // Read the response content
                    var responseContent = await response.Content.ReadFromJsonAsync<RawResponse>();
                    _logger.LogDebug("Received response from foxesscloud.com succesfully.");

                    if (_logger.IsEnabled(LogLevel.Trace))
                    {
                        var options = new JsonSerializerOptions
                        {
                            WriteIndented = true
                        };
                        _logger.LogTrace($"Response: {JsonSerializer.Serialize(responseContent, options)}");
                    }

                    if (responseContent.errno != 0)
                    {
                        throw responseContent.errno switch
                        {
                            41930 => new Exception("incorrect inverter Id."),
                            40261 => new Exception("incorrect inverter Id."),
                            41808 => new TokenExpiredException(),
                            41809 => new Exception("Invalid token."),
                            _ => new Exception($"Error while getting raw data. Error Code {responseContent.errno}")
                        };
                    }
                    return responseContent;
                }
                else
                {
                    throw new Exception("Request failed with status code: " + response.StatusCode);
                }
            }
        }

        private async Task GetTokenAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                _logger.LogDebug($"Aquiring access token from the FoxEssCloud...");
                var content = JsonContent.Create(new { user = _username, password = _hashedPassword });
                var response = await client.PostAsync("https://www.foxesscloud.com/c/v0/user/login", content);

                if (response.IsSuccessStatusCode)
                {
                    // Read the response content
                    var responseContent = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (responseContent.errno != 0)
                    {
                        throw responseContent.errno switch
                        {
                            41807 => new Exception("Wrong username or password."),
                            12345 => new Exception("Another specific error message."),
                            _ => new Exception($"Error while getting access token. Error Code {responseContent.errno}")
                        };
                    }
                    _logger.LogDebug("Access token aquired!");
                    _token = responseContent.result.token;
                }
                else
                {
                    throw new Exception("Request failed with status code: " + response.StatusCode);
                }
            }
        }
    }
}