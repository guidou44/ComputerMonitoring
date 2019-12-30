using HardwareManipulation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using NvAPIWrapper;
using NvAPIWrapper.Display;
using NvAPIWrapper.GPU;
using NvAPIWrapper.Mosaic;
using HardwareManipulation.Enums;

namespace HardwareManipulation.Connectors
{
    public class GPU_Connector : ConnectorBase
    {
        public GPU_Connector()
        {
            NVIDIA.Initialize();
        }

        ~GPU_Connector()
        {
            NVIDIA.Unload();
        }

        #region Private Methods

        private static HardwareInformation GetFirstGpuMake()
        {
            var myGPUs = PhysicalGPU.GetPhysicalGPUs();
            if (myGPUs.Count() == 0) return null;

            var gpuMakes = myGPUs.ToList().Select(gU => new HardwareInformation()
            {
                MainValue = gU.FullName,
                ShortName = "GPU",
                UnitSymbol = ""
            });

            return gpuMakes.FirstOrDefault();
        }

        private static HardwareInformation GetFirstGpuTemp()
        {
            var myGPUs = PhysicalGPU.GetPhysicalGPUs();
            if (myGPUs.Count() == 0) return null;

            var gpuUsages = myGPUs.ToList().Select(gU => new HardwareInformation()
            {
                ShortName = "GPU",
                MainValue = (double) gU.ThermalInformation.ThermalSensors.First().CurrentTemperature,
                UnitSymbol = "°C"
            });

            return gpuUsages.FirstOrDefault();
        }

        private static HardwareInformation GetFirstGpuUsage()
        {
            var myGPUs = PhysicalGPU.GetPhysicalGPUs();
            if (myGPUs.Count() == 0) return null;

            var gpuUsages = myGPUs.ToList().Select(gU => new HardwareInformation()
            { 
                ShortName = "GPU",
                MainValue = (double) gU.UsageInformation.GPU.Percentage,
                UnitSymbol = "%"
            });

            return gpuUsages.FirstOrDefault();
        }

        #endregion

        public override HardwareInformation GetValue(MonitoringTarget ressource)
        {
            switch (ressource)
            {
                case MonitoringTarget.GPU_Make:
                    return GetFirstGpuMake();

                case MonitoringTarget.GPU_Temp:
                    return GetFirstGpuTemp();

                case MonitoringTarget.GPU_Usage:
                    return GetFirstGpuUsage();

                default:
                    throw new NotImplementedException($"Monitoring target '{ressource}' is not implemented for gpu connector.");
            }
        }
    }
}
