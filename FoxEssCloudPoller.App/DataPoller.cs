using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Timer = System.Threading.Timer;

namespace FoxEssCloudPoller
{
    public class DataPoller : IHostedService
    {
        private FoxEssCloudClient _client;
        private IHandleNewInverterMeasurements _handler;
        private ILogger<DataPoller> _logger;
        private DateTime _processedUntil;
        private Timer? _timer;

        public DataPoller(FoxEssCloudClient client, IHandleNewInverterMeasurements handler, ILogger<DataPoller> logger)
        {
            _client = client;
            _handler = handler;
            _logger = logger;

            //substract 5 minutes so that there will always be at least one value that can be processed.
            _processedUntil = DateTime.Now.AddMinutes(-5);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("FoxEssCloudPoller starting...");

            TimerCallback callback = TimerCallbackMethod;
            _timer = new Timer(callback, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));

            _logger.LogInformation("Timer started. Press Ctrl-C to exit.");

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer.Dispose();
            _logger.LogInformation("Timer stopped.");
            _logger.LogInformation("FoxEssCloudPoller stopped gracefully.");

            return Task.CompletedTask;
        }

        private void TimerCallbackMethod(object? state)
        {
            try
            {
                GetData();
            }
            catch (Exception ex)
            {
                //never crash the program.
                _logger.LogError(ex, "An unexpected error occured.");
            }
        }

        private void GetData()
        {
            var rawValues = _client.GetHourlyRawValuesAsync(DateTime.Now).GetAwaiter().GetResult();

            //get the raw values from the previous hour if the current hour doesn't have any data yet.
            if (rawValues.result[0].data.Length == 0)
            {
                _logger.LogDebug("No data available in current hour yet, requesting previous hour instead.");
                rawValues = _client.GetHourlyRawValuesAsync(DateTime.Now.AddHours(-1)).GetAwaiter().GetResult();
            }

            //just getting the last value is not always correct, in rare occasions there are two values added.
            //get the values with a time greater than the last processed time.
            //this also allows for a partial self heal after a network outage. (as long as the outage was within one whole hour)
            var newValues = rawValues.result
                .Select(r => new { r.variable, data = r.data.Where(d => d.parsedTime > _processedUntil).ToArray() })
                .ToDictionary(d => d.variable);

            //assuming all variables have the same amount of values.
            var numberOfUnprocessedRawValues = newValues[FoxEssVariables.GenerationPower].data.Length;
            if (numberOfUnprocessedRawValues == 0)
            {
                _logger.LogDebug($"No new values found since last time {_processedUntil}.");
            }

            for (int i = 0; i < numberOfUnprocessedRawValues; i++)
            {
                var measurements = new InverterMeasurements
                {
                    Timestamp = newValues[FoxEssVariables.GenerationPower].data[i].parsedTime,
                    GeneratedPower = newValues[FoxEssVariables.GenerationPower].data[i].value * 1000, //convert from kW to W
                    InverterTemperature = (int)newValues[FoxEssVariables.InvTemperation].data[i].value, //can be casted to int because foxess only returns integer values
                    P1Volt = newValues[FoxEssVariables.PV1Volt].data[i].value,
                    P2Volt = newValues[FoxEssVariables.PV2Volt].data[i].value,
                    P3Volt = newValues[FoxEssVariables.PV3Volt].data[i].value,
                    P4Volt = newValues[FoxEssVariables.PV4Volt].data[i].value,
                };

                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };
                    _logger.LogTrace($"Final measurements that are passed down to the handler (e.g. sent to PVOutput):{Environment.NewLine}Measurements: {JsonSerializer.Serialize(measurements, options)}");
                }

                _handler.Handle(measurements);
                _processedUntil = measurements.Timestamp;
            }
        }
    }
}