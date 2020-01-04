using HardwareAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using NvAPIWrapper;
using NvAPIWrapper.Display;
using NvAPIWrapper.GPU;
using NvAPIWrapper.Mosaic;
using HardwareAccess.Enums;

namespace HardwareAccess.Connectors
{
    public class NVDIA_API_Connector : ConnectorBase
    {
        public NVDIA_API_Connector()
        {
            NVIDIA.Initialize();
        }

        ~NVDIA_API_Connector()
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

        private static HardwareInformation GetFirstGpuLoad()
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

                case MonitoringTarget.GPU_Load:
                    return GetFirstGpuLoad();

                default:
                    throw new NotImplementedException($"Monitoring target '{ressource}' is not implemented for connector {nameof(NVDIA_API_Connector)}");
            }
        }
    }
}
