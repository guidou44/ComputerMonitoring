using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace ComputerRessourcesMonitoring.Models
{
    public static class PerformanceInfo
    {
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

        public static CpuUsage GetGlobalDetailedCpuUsage()
        {
            var wmiObject = new ManagementObjectSearcher("select * from Win32_Processor");

            var cpuUsage = GetGlobalCpuUsage();


            return (cpuUsage != null) ? cpuUsage :
                throw new ArgumentNullException("No cpu usage was found in ManagementObjectSearcher");
        }

    }
}
