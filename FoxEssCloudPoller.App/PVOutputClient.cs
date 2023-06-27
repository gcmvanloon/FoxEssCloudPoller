using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Web;
using System.Collections.Specialized;
using System.Linq;

namespace FoxEssCloudPoller
{
    public class PVOutputClient
    {
        private readonly ILogger<PVOutputClient> _logger;

        private readonly string _apiKey;
        private readonly int _systemId;

        public PVOutputClient(IConfiguration configuration, ILogger<PVOutputClient> logger)
        {
            _apiKey = configuration.GetValue<string>("PVOutput:ApiKey");
            _systemId = configuration.GetValue<int>("PVOutput:SystemId");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task AddStatusAsync(InverterMeasurements status)
        {
            //the service used in this method is documented by PVOutput.org here:
            //https://pvoutput.org/help/api_specification.html#add-status-service

            using (HttpClient client = new HttpClient())
            {
                var content = new StringContent(string.Empty);
                content.Headers.Add("X-Pvoutput-Apikey", _apiKey);
                content.Headers.Add("X-Pvoutput-SystemId", _systemId.ToString());

                var queryString = string.Join("&", new Dictionary<string, string>()
                {
                    { "d", status.Timestamp.ToString("yyyyMMdd") },
                    { "t", status.Timestamp.ToString("HH:mm") },
                    { "v2", status.GeneratedPower.ToString() },
                    { "v5", status.InverterTemperature.ToString() },
                    { "v6", status.TotalPVolt().ToString() }
                }.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

                UriBuilder uriBuilder = new UriBuilder("https://pvoutput.org/service/r2/addstatus.jsp");
                uriBuilder.Query = queryString;

                _logger.LogInformation("Posting data to PVOutput.org...");
                _logger.LogDebug(uriBuilder.ToString());
                var response = await client.PostAsync(uriBuilder.Uri, content);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Unexpected HTTP StatusCode {response.StatusCode} from PVOutput received.");
                }
            }
        }
    }
}
