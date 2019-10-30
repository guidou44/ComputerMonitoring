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
            return GetEachCpuUsage(Win32_Processor : true).FirstOrDefault().Cpu_Usage;
        }


        public static ICollection<CpuUsage> GetEachCpuUsage()
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

        public static ICollection<CpuUsage> GetEachCpuUsage(bool Win32_Processor)
        {
            var wmiObject = new ManagementObjectSearcher("select * from Win32_Processor");

            var cpuUsage = wmiObject.Get()
                                   .Cast<ManagementObject>()
                                   .Select(mo => new CpuUsage
                                   {
                                       Cpu_Usage = Double.Parse(mo["LoadPercentage"].ToString())
                                   }
                                   )
                                   .ToList();

            return (cpuUsage.Count() != 0) ? cpuUsage :
                throw new ArgumentNullException("No cpu usage was found in ManagementObjectSearcher");
        }

        public static float GetCpuClockSpeed()
        {
            var wmiObject = new ManagementObjectSearcher("select * from Win32_Processor");

            var cpuClockSpeed = wmiObject.Get()
                                   .Cast<ManagementObject>()
                                   .Select(mo => new CpuUsage
                                   {
                                       Cpu_Name = mo["Name"].ToString(),
                                       Cpu_Current_ClockSpeed = 0.001f * UInt32.Parse(mo["CurrentClockSpeed"].ToString())
                                   }
                                   )
                                   .ToList();

            return (cpuClockSpeed.Count() > 0) ? cpuClockSpeed.FirstOrDefault().Cpu_Current_ClockSpeed : 
                throw new ArgumentNullException("No cpu usage was found in ManagementObjectSearcher");
        }

    }
}
