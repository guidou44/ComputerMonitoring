using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComputerRessourcesMonitoring.Models
{
    public static class PerformanceInfo
    {
        private static PerformanceCounter all_Cpu_Idle;

        public static double GetCurrentRamMemoryUsage()
        {
            var wmiObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");

            var memoryValues = wmiObject.Get().Cast<ManagementObject>().Select(mo => new {
                FreePhysicalMemory = Double.Parse(mo["FreePhysicalMemory"].ToString()),
                TotalVisibleMemorySize = Double.Parse(mo["TotalVisibleMemorySize"].ToString())
            }).FirstOrDefault();

            return (memoryValues != null) ? Math.Round(((memoryValues.TotalVisibleMemorySize - memoryValues.FreePhysicalMemory) / memoryValues.TotalVisibleMemorySize) * 100, 2) : 
            throw new ArgumentNullException("No memory was found in ManagementObjectSearcher"); ;
        }

        public static double GetCurrentTotalCpuUsage()
        {
            return GetGlobalCpuUsage().Cpu_Usage;
        }

        public static double GetCurrentTotalCpuUsage(bool usePerformanceCounter)
        {
            if (!usePerformanceCounter) return GetCurrentTotalCpuUsage();
            else
            {
                all_Cpu_Idle = (all_Cpu_Idle == null) ? new PerformanceCounter("Processor", "% Idle Time", "_Total") : all_Cpu_Idle;
                var cpuUsage = all_Cpu_Idle.NextValue();
                return Math.Round(100.0 - cpuUsage, 2);
            }
        }

        public static IEnumerable<CpuUsage> GetEachCpuUsage()
        {
             var wmiObject = new ManagementObjectSearcher("select * from Win32_PerfFormattedData_PerfOS_Processor");
             
             var allCpuUsage = wmiObject.Get()
                                    .Cast<ManagementObject>()
                                    .Select(mo => new CpuUsage
                                    {
                                        Cpu_Name = mo["Name"].ToString(),
                                        Cpu_Usage = Double.Parse(mo["PercentProcessorTime"].ToString())
                                    }
                                    )
                                    .ToList();

            return (allCpuUsage.Count() != 0)? allCpuUsage :
                throw new ArgumentNullException("No cpu usage was found in ManagementObjectSearcher");
        }

        public static CpuUsage GetGlobalCpuUsage()
        {
            var wmiObject = new ManagementObjectSearcher("select * from Win32_Processor");

            var cpuUsage = wmiObject.Get()
                                   .Cast<ManagementObject>()
                                   .Select(mo => new CpuUsage
                                   {
                                       Cpu_Name = mo["Name"].ToString(),
                                       Cpu_Usage = Double.Parse(mo["LoadPercentage"].ToString()),
                                       Cpu_Current_ClockSpeed = 0.001f * UInt32.Parse(mo["CurrentClockSpeed"].ToString()),
                                       Number_of_cores = UInt32.Parse(mo["NumberOfCores"].ToString()),
                                       Thread_count = UInt32.Parse(mo["ThreadCount"].ToString())
                                   }
                                   )
                                   .FirstOrDefault();

            return (cpuUsage != null) ? cpuUsage :
                throw new ArgumentNullException("No cpu usage was found in ManagementObjectSearcher");
        }
    }
}
