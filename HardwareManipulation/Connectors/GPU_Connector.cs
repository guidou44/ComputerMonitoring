using HardwareManipulation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using NvAPIWrapper;
using NvAPIWrapper.Display;
using NvAPIWrapper.GPU;
using NvAPIWrapper.Mosaic;

namespace HardwareManipulation.Connectors
{
    public static class GPU_Connector
    {
        public static void InitializeGpuWatcher()
        {
            NVIDIA.Initialize();
        }

        public static HardwareUsageBase GetFirstGpuInformation()
        {
            var myGPUs = PhysicalGPU.GetPhysicalGPUs();
            if (myGPUs.Count() == 0) return null;

            var gpuUsages = myGPUs.ToList().Select(gU => new GpuUsage()
            { 
                Id = gU.GPUId,
                Name = gU.FullName,
                Temperature = gU.ThermalInformation.ThermalSensors.First().CurrentTemperature,
                Main_Value =gU.UsageInformation.GPU.Percentage
            });

            return gpuUsages.FirstOrDefault();
        }

        public static HardwareUsageBase GetFirstGpuTempOnly()
        {
            var myGPUs = PhysicalGPU.GetPhysicalGPUs();
            if (myGPUs.Count() == 0) return null;

            var gpuUsages = myGPUs.ToList().Select(gU => new GpuUsage()
            {
                Id = gU.GPUId,
                Name = gU.FullName,
                Temperature = gU.ThermalInformation.ThermalSensors.First().CurrentTemperature,
                Main_Value = gU.UsageInformation.GPU.Percentage
            });

            return new GpuTemp() { Main_Value = (GetFirstGpuInformation() as GpuUsage).Temperature };
        }

        public static void ResetGpuWatcher()
        {
            NVIDIA.Unload();
        }
    }
}
