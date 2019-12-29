using HardwareManipulation.Enums;
using HardwareManipulation.Helpers;
using HardwareManipulation.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HardwareManipulation.Connectors
{
    public class CPU_Connector : ConnectorBase
    {
        private static PerformanceCounter all_Cpu_Idle;


        #region Public Methods

        public static HardwdareInformation GetCurrentGlobalCpuUsageWithPerfCounter()
        {
                all_Cpu_Idle = (all_Cpu_Idle == null) ? new PerformanceCounter("Processor", "% Idle Time", "_Total") : all_Cpu_Idle;
                var cpuIdle = all_Cpu_Idle.NextValue();
                return new HardwdareInformation() 
                { 
                    Main_Value = Math.Round(100.0 - cpuIdle, 2),
                    ShortName = "CPU",
                    UnitSymbol = "%"
                };
        }

        public static HardwdareInformation GetCpuTemperature()
        {
            var cpuTemp = WmiHelper.GetWmiValue<double>("MSAcpi_ThermalZoneTemperature", "CurrentTemperature", scope: @"root\WMI");
            var cpuUsage = new HardwdareInformation()
            {
                ShortName = "CPU",
                Main_Value = (cpuTemp - 2732) / 10.0,
                UnitSymbol = "°C"
            };

            return cpuUsage;
        }

        public static HardwdareInformation GetCpuThreadCount()
        {
            var threadCount = WmiHelper.GetWmiValue<uint>("Win32_Processor", "ThreadCount");
            var cpuThreadCount = new HardwdareInformation()
            {
                Main_Value = threadCount,
                ShortName = "CPU",
                UnitSymbol = "Threads"
            };

            return (cpuThreadCount != null) ? cpuThreadCount :
                throw new ArgumentNullException("No cpu usage was found in ManagementObjectSearcher");
        }

        public static HardwdareInformation GetCurrentGlobalCpuUsage()
        {
            var loadPercentage = WmiHelper.GetWmiValue<double>("Win32_Processor", "LoadPercentage");
            var cpuUsage = new HardwdareInformation()
            { 
                Main_Value = loadPercentage,
                ShortName = "CPU",
                UnitSymbol = "%"
            };

            //Current_ClockSpeed = 0.001f * UInt32.Parse(mo["CurrentClockSpeed"].ToString()),
            //Number_of_cores = UInt32.Parse(mo["NumberOfCores"].ToString()),

            return (cpuUsage != null) ? cpuUsage :
                throw new ArgumentNullException("No cpu usage was found in ManagementObjectSearcher");
        }

        public static IEnumerable<HardwdareInformation> GetEachCpuUsage()
        {
             var wmiObject = new ManagementObjectSearcher("select * from Win32_PerfFormattedData_PerfOS_Processor");
             
             var allCpuUsage = wmiObject.Get()
                                    .Cast<ManagementObject>()
                                    .Select(mo => new HardwdareInformation
                                    {
                                        ShortName = "CPU" + mo["Name"].ToString(),
                                        Main_Value = Double.Parse(mo["PercentProcessorTime"].ToString()),
                                        UnitSymbol = "%"
                                    }
                                    )
                                    .ToList();

            return (allCpuUsage.Count() != 0)? allCpuUsage :
                throw new ArgumentNullException("No cpu usage was found in ManagementObjectSearcher");
        }

        public override HardwdareInformation GetValue(MonitoringTarget ressource)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
