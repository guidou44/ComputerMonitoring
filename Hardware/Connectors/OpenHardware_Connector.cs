using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hardware.Enums;
using Hardware.Models;
using Hardware.Components;
using Hardware.Wrappers;
using OpenHardwareMonitor.Hardware;

namespace Hardware.Connectors
{
    public class OpenHardware_Connector : ConnectorBase
    {
        private readonly IOpenHardwareComputer _localMachine;
        public OpenHardware_Connector(IOpenHardwareComputer computer)
        {
            _localMachine = computer;
            _localMachine.MainboardEnabled = true;
            _localMachine.GPUEnabled = true;
            _localMachine.Open();
        }

        ~OpenHardware_Connector()
        {
            _localMachine.Close();
        }

        #region Private Methods

        private HardwareInformation GetGpuMake()
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
                if (hardware.HardwareType == OpenHardwareType.GpuAti || hardware.HardwareType == OpenHardwareType.GpuNvidia)
                {
                    output.MainValue = hardware.Name;
                    break;
                }
            }

            return output;
        }

        private HardwareInformation GetHardwareSensorValue<T>(string unitSymbol, string sensorRegionName, OpenHardwareSensorType sensorType)
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
                if (hardware.HardwareType == OpenHardwareType.GpuAti || hardware.HardwareType == OpenHardwareType.GpuNvidia)
                {
                    var reading = hardware.Sensors.Where(S => S.SensorType == sensorType && S.Name == sensorRegionName).SingleOrDefault().Value;
                    if (reading != null) output.MainValue = Convert.ChangeType(Math.Round((double)reading, 2), typeof(T));
                    break;
                }
            }

            return output;
        }


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
                            if (sensor.SensorType == OpenHardwareSensorType.Fan && sensor.Name.Equals("Fan #1", StringComparison.OrdinalIgnoreCase))
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

        private HardwareInformation GetMotherBoardMake()
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
                if (hardware.HardwareType == OpenHardwareType.Mainboard)
                {
                    output.MainValue = hardware.Name;
                    break;
                }
            }

            return output;
        }

        #endregion

        public override HardwareInformation GetValue(MonitoringTarget resource)
        {
            switch (resource)
            {
                case MonitoringTarget.GPU_Clock:
                    return GetHardwareSensorValue<double>("MHz", "GPU Core", OpenHardwareSensorType.Clock);

                case MonitoringTarget.GPU_Make:
                    return GetGpuMake();

                case MonitoringTarget.GPU_Memory_Controller:
                    return GetHardwareSensorValue<double>("Mem.Controllers", "GPU Memory Controller", OpenHardwareSensorType.Load);

                case MonitoringTarget.GPU_Memory_Clock:
                    return GetHardwareSensorValue<double>("MHz Mem.Clock", "GPU Memory", OpenHardwareSensorType.Clock);

                case MonitoringTarget.GPU_Memory_Load:
                    return GetHardwareSensorValue<double>("%", "GPU Memory", OpenHardwareSensorType.Load);

                case MonitoringTarget.GPU_Shader_Clock:
                    return GetHardwareSensorValue<double>("MHz Shader.Clock", "GPU Shader", OpenHardwareSensorType.Clock);

                case MonitoringTarget.GPU_Temp:
                    return GetHardwareSensorValue<double>("°C", "GPU Core", OpenHardwareSensorType.Temperature);

                case MonitoringTarget.GPU_Load:
                    return GetHardwareSensorValue<double>("%", "GPU Core", OpenHardwareSensorType.Load); 

                case MonitoringTarget.GPU_VideoEngine_Load:
                    return GetHardwareSensorValue<double>("%", "GPU Video Engine", OpenHardwareSensorType.Load);

                case MonitoringTarget.FAN_Speed:
                    return GetMainFanSpeed();

                case MonitoringTarget.Mother_Board_Make:
                    return GetMotherBoardMake();

                default:
                    throw new NotImplementedException($"Monitoring target '{resource}' not implemented for connector {nameof(OpenHardware_Connector)}");
            }
        }
    }
}
