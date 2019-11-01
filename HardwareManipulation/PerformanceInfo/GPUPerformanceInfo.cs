using HardwareManipulation.Models;
using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareManipulation.HardwareInformation
{
    public static class GPUPerformanceInfo
    {
        private static Computer localComputer;

        public static void InitializeGpuWatcher()
        {
            localComputer = new Computer();
            localComputer.GPUEnabled = true;
            localComputer.Open();
        }

        public static IEnumerable<GpuUsage> GetGpuTemperature()
        {
            var gpuHardware = localComputer.Hardware.Where(H => H.HardwareType == HardwareType.GpuNvidia)
                                                    .FirstOrDefault();
            if (gpuHardware == null) return null;

            var gpuUsages =  gpuHardware.Sensors
                                        .Where(S => S.SensorType == SensorType.Temperature)
                                        .Select(S => new GpuUsage() { Temperature = Math.Round(Double.Parse(S.Value.ToString()), 2),
                                                                      Name = gpuHardware.Name});

            return (gpuUsages.Count() != 0) ? gpuUsages : throw new ArgumentNullException("Unable to find gpu information");
        }

        public static void ResetGpuWatcher()
        {
            localComputer.Close();
            localComputer = null;
        }
    }
}
