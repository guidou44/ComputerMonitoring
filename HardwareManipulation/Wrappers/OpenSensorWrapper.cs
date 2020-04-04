using HardwareManipulation.Components;
using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareManipulation.Wrappers
{
    public class OpenSensorWrapper : IOpenSensor
    {
        private ISensor _sensor;
        public OpenSensorWrapper(ISensor sensor)
        {
            _sensor = sensor;
        }

        public string Name
        {
            get { return _sensor.Name; }
        }

        public float? Value
        {
            get { return _sensor.Value; }
        }

        public OpenHardwareSensorType SensorType
        {
            get { return MapSensorType(_sensor.SensorType); }
        }

        private OpenHardwareSensorType MapSensorType(SensorType type)
        {
            switch (type)
            {
                case OpenHardwareMonitor.Hardware.SensorType.Clock:
                    return OpenHardwareSensorType.Clock;
                case OpenHardwareMonitor.Hardware.SensorType.Temperature:
                    return OpenHardwareSensorType.Temperature;
                case OpenHardwareMonitor.Hardware.SensorType.Load:
                    return OpenHardwareSensorType.Load;
                case OpenHardwareMonitor.Hardware.SensorType.Fan:
                    return OpenHardwareSensorType.Fan;
                default:
                    throw new NotImplementedException(type.ToString());
            }
        }
    }
}
