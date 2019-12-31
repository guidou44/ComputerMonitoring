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
    public class OpenHardware_Connector : ConnectorBase
    {
        private static Computer _localMachine;
        public OpenHardware_Connector()
        {
            if (_localMachine == null) _localMachine = new Computer();
            _localMachine.MainboardEnabled = true;
            _localMachine.GPUEnabled = true;
            _localMachine.Open();
        }

        ~OpenHardware_Connector()
        {
            _localMachine.Close();
        }

        #region Private Methods

        private static HardwareInformation GetGpuMake()
        {
            HardwareInformation output = new HardwareInformation()
            {
                MainValue = null,
                ShortName = "GPU",
                UnitSymbol = ""
            };

            foreach (var hardware in _localMachine.Hardware)
            {
                hardware.Update();
                if (hardware.HardwareType == HardwareType.GpuAti || hardware.HardwareType == HardwareType.GpuNvidia)
                {
                    output.MainValue = hardware.Name;
                    break;
                }
            }

            return output;
        }

        private static HardwareInformation GetHardwareSensorValue<T>(string unitSymbol, string sensorRegionName, SensorType sensorType)
        {
            HardwareInformation output = new HardwareInformation()
            {
                MainValue = null,
                ShortName = "GPU",
                UnitSymbol = unitSymbol
            };

            foreach (var hardware in _localMachine.Hardware)
            {
                hardware.Update();
                if (hardware.HardwareType == HardwareType.GpuAti || hardware.HardwareType == HardwareType.GpuNvidia)
                {
                    var reading = hardware.Sensors.Where(S => S.SensorType == sensorType && S.Name == sensorRegionName).SingleOrDefault().Value;
                    if (reading != null) output.MainValue = Convert.ChangeType(Math.Round((double)reading, 2), typeof(T));
                    break;
                }
            }

            return output;
        }


        private static HardwareInformation GetMainFanSpeed()
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

        private static HardwareInformation GetMotherBoardMake()
        {
            HardwareInformation output = new HardwareInformation()
            {
                MainValue = null,
                ShortName = "MotherBoard",
                UnitSymbol = ""
            };

            foreach (var hardware in _localMachine.Hardware)
            {
                hardware.Update();
                if (hardware.HardwareType == HardwareType.Mainboard)
                {
                    output.MainValue = hardware.Name;
                    break;
                }
            }

            return output;
        }

        #endregion

        public override HardwareInformation GetValue(MonitoringTarget ressource)
        {
            switch (ressource)
            {
                case MonitoringTarget.GPU_Clock:
                    return GetHardwareSensorValue<double>("MHz", "GPU Core", SensorType.Clock);

                case MonitoringTarget.GPU_Make:
                    return GetGpuMake();

                case MonitoringTarget.GPU_Memory_Controller:
                    return GetHardwareSensorValue<double>("Mem.Controllers", "GPU Memory Controller", SensorType.Load);

                case MonitoringTarget.GPU_Memory_Clock:
                    return GetHardwareSensorValue<double>("MHz", "GPU Memory", SensorType.Clock);

                case MonitoringTarget.GPU_Memory_Load:
                    return GetHardwareSensorValue<double>("%", "GPU Memory", SensorType.Load);

                case MonitoringTarget.GPU_Shader_Clock:
                    return GetHardwareSensorValue<double>("MHz", "GPU Shader", SensorType.Clock);

                case MonitoringTarget.GPU_Temp:
                    return GetHardwareSensorValue<double>("°C", "GPU Core", SensorType.Temperature);

                case MonitoringTarget.GPU_Load:
                    return GetHardwareSensorValue<double>("%", "GPU Core", SensorType.Load); 

                case MonitoringTarget.GPU_VideoEngine_Load:
                    return GetHardwareSensorValue<double>("%", "GPU Video Engine", SensorType.Load);

                case MonitoringTarget.FAN_Speed:
                    return GetMainFanSpeed();

                case MonitoringTarget.Mother_Board_Make:
                    return GetMotherBoardMake();

                default:
                    throw new NotImplementedException($"Monitoring target '{ressource}' not implemented for connector {nameof(OpenHardware_Connector)}");
            }
        }
    }
}
