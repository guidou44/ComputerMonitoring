using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HardwareManipulation.Enums;
using HardwareManipulation.Models;
using OpenHardwareMonitor.Hardware;

namespace HardwareManipulation.Connectors
{
    public class FAN_Connector : ConnectorBase
    {
        private Computer _localMachine;
        public FAN_Connector()
        {
            _localMachine = new Computer();
            _localMachine.MainboardEnabled = true;
            _localMachine.GPUEnabled = true;
            _localMachine.Open();
        }

        ~FAN_Connector()
        {
            _localMachine.Close();
        }

        #region Private Methods

        private HardwareInformation GetMainFanSpeed()
        {
            HardwareInformation cpuFanSpeed = new HardwareInformation()
            {
                MainValue = null,
                ShortName = "FAN",
                UnitSymbol = "RPM"
            };

            foreach (var hardware in _localMachine.Hardware)
            {
                foreach (var subhardware in hardware.SubHardware)
                {
                    subhardware.Update();
                    if (subhardware.Sensors.Length > 0)
                    {
                        foreach (var sensor in subhardware.Sensors)
                        {
                            if (sensor.SensorType == SensorType.Fan && sensor.Name.Equals("Fan #1", StringComparison.OrdinalIgnoreCase))
                            {
                                var test = sensor;
                                cpuFanSpeed.MainValue = ((double)(float)sensor.Value);
                            }
                        }
                    }
                }
            }

            return cpuFanSpeed;
        }

        #endregion

        public override HardwareInformation GetValue(MonitoringTarget ressource)
        {
            switch (ressource)
            {
                case MonitoringTarget.FAN_Speed:
                    return GetMainFanSpeed();

                default:
                    throw new NotImplementedException($"Monitoring target '{ressource}' is not implemented for connector {nameof(FAN_Connector)}"); 
            }
        }
    }
}
