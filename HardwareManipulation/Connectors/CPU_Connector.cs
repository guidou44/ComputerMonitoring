using HardwareManipulation.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HardwareManipulation.Connectors
{
    public static class CPU_Connector
    {
        private static PerformanceCounter all_Cpu_Idle;

        public static HardwareUsageBase GetCurrentGlobalCpuUsageWithPerfCounter()
        {
                all_Cpu_Idle = (all_Cpu_Idle == null) ? new PerformanceCounter("Processor", "% Idle Time", "_Total") : all_Cpu_Idle;
                var cpuIdle = all_Cpu_Idle.NextValue();
                return new CpuUsage() { Main_Value = Math.Round(100.0 - cpuIdle, 2) };
        }

        public static IEnumerable<CpuUsage> GetEachCpuUsage()
        {
             var wmiObject = new ManagementObjectSearcher("select * from Win32_PerfFormattedData_PerfOS_Processor");
             
             var allCpuUsage = wmiObject.Get()
                                    .Cast<ManagementObject>()
                                    .Select(mo => new CpuUsage
                                    {
                                        Name = mo["Name"].ToString(),
                                        Main_Value = Double.Parse(mo["PercentProcessorTime"].ToString())
                                    }
                                    )
                                    .ToList();

            return (allCpuUsage.Count() != 0)? allCpuUsage :
                throw new ArgumentNullException("No cpu usage was found in ManagementObjectSearcher");
        }

        public static HardwareUsageBase GetCurrentGlobalCpuUsage()
        {
            var wmiObject = new ManagementObjectSearcher("select * from Win32_Processor");

            var cpuUsage = wmiObject.Get()
                                   .Cast<ManagementObject>()
                                   .Select(mo => new CpuUsage
                                   {
                                       Name = mo["Name"].ToString(),
                                       Main_Value = Double.Parse(mo["LoadPercentage"].ToString()),
                                       Current_ClockSpeed = 0.001f * UInt32.Parse(mo["CurrentClockSpeed"].ToString()),
                                       Number_of_cores = UInt32.Parse(mo["NumberOfCores"].ToString()),
                                       Thread_count = UInt32.Parse(mo["ThreadCount"].ToString())
                                   }
                                   )
                                   .FirstOrDefault();

            return (cpuUsage != null) ? cpuUsage :
                throw new ArgumentNullException("No cpu usage was found in ManagementObjectSearcher");
        }

        public static HardwareUsageBase GetCpuTemperature()
        {
            var wmiObject = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature");
            var cpuUsage = wmiObject.Get()
                                   .Cast<ManagementObject>()
                                   .Select(mo => new CpuUsage
                                   {
                                       Name = mo["InstanceName"].ToString(),
                                       Temperature = (Double.Parse(mo["CurrentTemperature"].ToString()) - 2732) / 10.0
                                   }
                                   );

            return new CpuTemp() { Main_Value = cpuUsage.FirstOrDefault().Temperature };
        }
    }
}
