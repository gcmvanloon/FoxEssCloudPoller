using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Timer = System.Threading.Timer;

namespace FoxEssCloudPoller
{
    public class DataPoller : IHostedService
    {
        private FoxEssOpenApiClient _client;
        private IHandleNewInverterMeasurements _handler;
        private ILogger<DataPoller> _logger;
        private DateTime _processedUntil;
        private Timer? _timer;

        public DataPoller(FoxEssOpenApiClient client, IHandleNewInverterMeasurements handler, ILogger<DataPoller> logger)
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
            _timer = new Timer(callback, null, TimeSpan.Zero, TimeSpan.FromMinutes(3));

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
            var realtimeData = _client.GetRealtimeDataAsync().GetAwaiter().GetResult();
            var result = realtimeData.Result[0];

            var measurements = new InverterMeasurements
            {
                Timestamp = result.Time.ToDateTime(),
                GeneratedPower = result.Datas.Find(d => d.Variable == FoxEssVariables.GenerationPower).Value * 1000, //convert from kW to W
                InverterTemperature = (int)Math.Round(result.Datas.Find(d => d.Variable == FoxEssVariables.InvTemperation).Value),
                P1Volt = result.Datas.Find(d => d.Variable == FoxEssVariables.PV1Volt).Value,
                P2Volt = result.Datas.Find(d => d.Variable == FoxEssVariables.PV2Volt).Value,
                P3Volt = result.Datas.Find(d => d.Variable == FoxEssVariables.PV3Volt).Value,
                P4Volt = result.Datas.Find(d => d.Variable == FoxEssVariables.PV4Volt).Value,
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