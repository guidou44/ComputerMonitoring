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
        public static void InitializeGpuWatcher()
        {
            NVIDIA.Initialize();
        }

        public static HardwdareInformation GetFirstGpuUsage()
        {
            var myGPUs = PhysicalGPU.GetPhysicalGPUs();
            if (myGPUs.Count() == 0) return null;

            var gpuUsages = myGPUs.ToList().Select(gU => new HardwdareInformation()
            { 
                ShortName = "GPU",
                Main_Value =gU.UsageInformation.GPU.Percentage,
                UnitSymbol = "%"
            });

            return gpuUsages.FirstOrDefault();
        }

        public static HardwdareInformation GetFirstGpuTemp()
        {
            var myGPUs = PhysicalGPU.GetPhysicalGPUs();
            if (myGPUs.Count() == 0) return null;

            var gpuUsages = myGPUs.ToList().Select(gU => new HardwdareInformation()
            {
                ShortName = "GPU",
                Main_Value = gU.ThermalInformation.ThermalSensors.First().CurrentTemperature,
                UnitSymbol = "°C"
            });

            return gpuUsages.FirstOrDefault();
        }

        public static void ResetGpuWatcher()
        {
            NVIDIA.Unload();
        }

        public override HardwdareInformation GetValue(MonitoringTarget ressource)
        {
            throw new NotImplementedException();
        }
    }
}
