using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace FoxEssCloudPoller
{
    public class FoxEssOpenApiClient
    {
        private readonly string _token;
        private readonly string _deviceSerialNr;
        private readonly ILogger<FoxEssOpenApiClient> _logger;

        private const string BaseUrl = "https://www.foxesscloud.com";

        public FoxEssOpenApiClient(IConfiguration configuration, ILogger<FoxEssOpenApiClient> logger)
        {
            ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

            //TODO: hanlde configutation errors
            _token = configuration.GetValue<string>("FoxEssCloud:ApiKey");
            _deviceSerialNr = configuration.GetValue<string>("FoxEssCloud:DeviceSerialNr");
            _logger = logger;
        }

        public async Task<RealQueryResponse> GetRealtimeDataAsync()
        {
            var path = "/op/v0/device/real/query";

            using (var client = new CustomHttpClient())
            {
                client.BeforeSend += SignRequest;
                var content = JsonContent.Create(new
                {
                    sn = _deviceSerialNr,
                    variables = new string[] {
                        FoxEssVariables.PVPower,
                        FoxEssVariables.AmbiantTemperation,
                        FoxEssVariables.PV1Volt,
                        FoxEssVariables.PV2Volt,
                        FoxEssVariables.PV3Volt,
                        FoxEssVariables.PV4Volt
                    }
                });

                _logger.LogDebug("Posting request to foxesscloud.com for realtime data...");
                var response = await client.PostAsync(BaseUrl + path, content);

                if (response.IsSuccessStatusCode)
                {
                    // Read the response content
                    var responseContent = await response.Content.ReadFromJsonAsync<RealQueryResponse>();
                    if (responseContent == null)
                    {
                        throw new Exception("Unable to succesfully deserialize the response into a RealQueryResponse object.");
                    }
                    _logger.LogDebug("Received response from foxesscloud.com succesfully.");

                    if (_logger.IsEnabled(LogLevel.Trace))
                    {
                        var options = new JsonSerializerOptions
                        {
                            WriteIndented = true
                        };
                        _logger.LogTrace($"Response: {JsonSerializer.Serialize(responseContent, options)}");
                    }

                    if (responseContent.Errno != 0)
                    {
                        throw new OpenApiException(responseContent.Errno, responseContent.Msg);
                    }
                    return responseContent;
                }
                else
                {
                    throw new Exception("Request failed with status code: " + response.StatusCode);
                }
            }
        }

        private void SignRequest(string url, HttpContent content)
        {
            var path = new Uri(url).AbsolutePath;
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var signature = CreateSignature(_token, timestamp, path);

            content.Headers.Add("token", _token);
            content.Headers.Add("timestamp", timestamp.ToString());
            content.Headers.Add("signature", signature);
            content.Headers.Add("lang", "en");
        }

        private string CreateSignature(string token, long timestamp, string path)
        {
            var signature = $@"{path}\r\n{_token}\r\n{timestamp}";
            //create a hash of the signature string using the MD5 algorithm
            return CreateHash(signature);
        }

        private string CreateHash(string inputString)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputString);

            // Create an instance of the MD5 algorithm
            using (MD5 md5 = MD5.Create())
            {
                // Compute the hash value from the input bytes
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to a hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }

                // Output the MD5 hash
                return sb.ToString();
            }
        }
    }
}