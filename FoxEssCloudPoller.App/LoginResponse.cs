using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxEssCloudPoller
{
    public class LoginResponse
    {
        public class Result
        {
            public string token { get; set; }
            public int access { get; set; }
            public string user { get; set; }
        }

        public int errno { get; set; }
        public Result result { get; set; }

    }
}
