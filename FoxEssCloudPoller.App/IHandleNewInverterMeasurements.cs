using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxEssCloudPoller
{
    public interface IHandleNewInverterMeasurements
    {
        void Handle(InverterMeasurements measurements);
    }
}
