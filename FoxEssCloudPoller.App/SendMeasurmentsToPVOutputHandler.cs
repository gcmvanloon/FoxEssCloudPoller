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
        PVOutputClient _pvOutputClient;
        ILogger<SendMeasurmentsToPVOutputHandler> _logger;

        public SendMeasurmentsToPVOutputHandler(PVOutputClient pvOutputClient, ILogger<SendMeasurmentsToPVOutputHandler> logger)
        {
            _previousMeasurements = new InverterMeasurements()
            {
                Timestamp = DateTime.MinValue
            };
            _pvOutputClient = pvOutputClient;
            _logger = logger;
        }

        public void Handle(InverterMeasurements measurements)
        {
            _logger.LogInformation("Sending measurements to PVOutput.org...");

            var previousPVOutputTime = new IntervalTime(_previousMeasurements.Timestamp, 5);
            var currentPVOutputTime = new IntervalTime(measurements.Timestamp, 5);

            //From the PVOutput docs: "Time data is rounded to the nearest interval set by the system.
            //For a system using a 5-minute status interval uploads via the Add Status and Add Batch Status services
            //with timestamp 09:58, 09:59, 10:00, 10:01, 10:02 will be adjusted to 10:00."
            //If two timestamps are adjusted to the same interval time, then the highest value must be kept for all variables.
            if (previousPVOutputTime.Equals(currentPVOutputTime))
            {
                _logger.LogDebug($"The previous ({previousPVOutputTime}) and the current ({currentPVOutputTime}) measurement are both on the same 5 minute interval for PVOutput. Keeping highest value for each variable.");
                measurements.GeneratedPower = Math.Max(measurements.GeneratedPower, _previousMeasurements.GeneratedPower);
                measurements.InverterTemperature = Math.Max(measurements.InverterTemperature, _previousMeasurements.InverterTemperature);
                measurements.P1Volt = Math.Max(measurements.P1Volt, _previousMeasurements.P1Volt);
                measurements.P2Volt = Math.Max(measurements.P2Volt, _previousMeasurements.P2Volt);
                measurements.P3Volt = Math.Max(measurements.P3Volt, _previousMeasurements.P3Volt);
                measurements.P4Volt = Math.Max(measurements.P4Volt, _previousMeasurements.P4Volt);
            }
            
            //just dump the values for now...
            _logger.LogInformation($"{currentPVOutputTime} | {measurements.GeneratedPower,10}W {measurements.InverterTemperature, 4}C {measurements.P1Volt, 6}V {measurements.P2Volt,6}V {measurements.P3Volt,6}V {measurements.P4Volt,6}V");

            _pvOutputClient.AddStatusAsync(measurements);

            _previousMeasurements = measurements;
        }
    }
}
