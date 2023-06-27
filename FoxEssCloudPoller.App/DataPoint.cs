using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxEssCloudPoller
{
    public class DataPoint
    {
        public string Variable { get; set; }
        public IntervalTime Timestamp { get; set; }
        public decimal Value { get; set; }

        public DataPoint(string variable, IntervalTime timestamp, decimal value)
        {
            Variable = variable;
            Timestamp = timestamp;
            Value = value;  
        }

        public override string ToString()
        {
            return $"{Variable,20} | {Timestamp} | {Value, 18}";
        }
    }
}
