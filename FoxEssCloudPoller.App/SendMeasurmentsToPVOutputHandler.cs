using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxEssCloudPoller
{
    public class SendMeasurmentsToPVOutputHandler : IHandleNewInverterMeasurements
    {
        private InverterMeasurements _previousMeasurements;
        private PVOutputClient _pvOutputClient;
        private ILogger<SendMeasurmentsToPVOutputHandler> _logger;

        public SendMeasurmentsToPVOutputHandler(PVOutputClient pvOutputClient, ILogger<SendMeasurmentsToPVOutputHandler> logger)
        {
            _previousMeasurements = new InverterMeasurements()
            {
                Timestamp = DateTime.MinValue
            };
            _pvOutputClient = pvOutputClient;
            _logger = logger;
        }

        public async void Handle(InverterMeasurements measurements)
        {
            _logger.LogInformation("Sending measurements to PVOutput.org...");

            var previousPVOutputTime = new IntervalTime(_previousMeasurements.Timestamp, 5);
            var currentPVOutputTime = new IntervalTime(measurements.Timestamp, 5);

            //From the PVOutput docs: "Time data is rounded to the nearest interval set by the system.
            //For a system using a 5-minute status interval uploads via the Add Status and Add Batch Status services
            //with timestamp 09:58, 09:59, 10:00, 10:01, 10:02 will be adjusted to 10:00."
            //If two timestamps are adjusted to the same interval time, then the average value must be kept for all variables.
            if (previousPVOutputTime.Equals(currentPVOutputTime))
            {
                _logger.LogDebug($"The previous ({previousPVOutputTime}) and the current ({currentPVOutputTime}) measurement are both on the same 5 minute interval for PVOutput. Keeping average value for each variable.");
                measurements.GeneratedPower = OverlappingIntervalStrategy(_previousMeasurements.GeneratedPower, measurements.GeneratedPower);
                measurements.InverterTemperature = OverlappingIntervalStrategy(_previousMeasurements.InverterTemperature, measurements.InverterTemperature);
                measurements.P1Volt = OverlappingIntervalStrategy(_previousMeasurements.P1Volt, measurements.P1Volt);
                measurements.P2Volt = OverlappingIntervalStrategy(_previousMeasurements.P2Volt, measurements.P2Volt);
                measurements.P3Volt = OverlappingIntervalStrategy(_previousMeasurements.P3Volt, measurements.P3Volt);
                measurements.P4Volt = OverlappingIntervalStrategy(_previousMeasurements.P4Volt, measurements.P4Volt);
            }

            //just dump the values for now...
            _logger.LogInformation($"{currentPVOutputTime} | {measurements.GeneratedPower,10}W {measurements.InverterTemperature,4}C {measurements.P1Volt,6}V {measurements.P2Volt,6}V {measurements.P3Volt,6}V {measurements.P4Volt,6}V");

            await _pvOutputClient.AddStatusAsync(measurements);

            _previousMeasurements = measurements;
        }

        private decimal OverlappingIntervalStrategy(decimal previousValue, decimal currentValue)
        {
            return (previousValue + currentValue) / 2;
        }

        private int OverlappingIntervalStrategy(int previousValue, int currentValue)
        {
            return (int)Math.Round((previousValue + currentValue) / 2.0);
        }
    }
}