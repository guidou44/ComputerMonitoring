using Hardware.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hardware.Components
{
    public interface IOpenSensor
    {
        string Name { get; }
        float? Value { get; }
        OpenHardwareSensorType SensorType { get; }
    }
}
