using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxEssCloudPoller
{
    public class RawResponse
    {
        public class Result
        {
            public class Data
            {
                private string? _time;
                private DateTime _parsedTime;

                public string? time 
                {
                    get => _time;
                    set
                    {
                        _time = value;
                        _parsedTime = DateTime.ParseExact(value[..19], "yyyy-MM-dd HH:mm:ss", null, System.Globalization.DateTimeStyles.AssumeLocal);
                    }
                }
                public decimal value { get; set; }
                public DateTime parsedTime { get => _parsedTime; }
            }

            public string variable { get; set; }
            public string unit { get; set; }
            public string name { get; set; }
            public Data[] data { get; set; }
        }

        public int errno { get; set; }
        public Result[] result { get; set; }

    }
}
