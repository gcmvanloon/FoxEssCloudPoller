using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxEssCloudPoller
{
    public class InverterMeasurements
    {
        public DateTime Timestamp { get; set; }
        public decimal GeneratedPower { get; set; }
        public int InverterTemperature { get; set; }
        public decimal P1Volt { get; set; }
        public decimal P2Volt { get; set; }
        public decimal P3Volt { get; set; }
        public decimal P4Volt { get; set; }

        public decimal TotalPVolt()
        {
            return P1Volt + P2Volt + P3Volt + P4Volt;
        }
    }
}
