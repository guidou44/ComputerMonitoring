using Common.Helpers;
using HardwareManipulation.Enums;
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
    public class WMI_Connector : ConnectorBase
    {
        private static PerformanceCounter all_Cpu_Idle;

        #region Private Methods

        private static HardwareInformation GetCpuCoreCount()
        {
            var numOfCore = WmiHelper.GetWmiValue<uint>("Win32_Processor", "NumberOfCores");
            var cpuCoreCount = new HardwareInformation()
            {
                MainValue = (double) numOfCore,
                ShortName = "CPU",
                UnitSymbol = (numOfCore > 1) ? "Cores" : "Core",
            };
            return cpuCoreCount;
        }

        private static HardwareInformation GetCpuClockSpeed()
        {
            var clockSpeed = WmiHelper.GetWmiValue<uint>("Win32_Processor", "CurrentClockSpeed");
            var cpuClockSpeed = new HardwareInformation()
            {
                MainValue = (double) (clockSpeed * 001f)/1000,
                ShortName = "CPU",
                UnitSymbol = "kHz",
            };
            return cpuClockSpeed;
        }

        private static HardwareInformation GetCpuMake()
        {
            var make = WmiHelper.GetWmiValue<string>("Win32_Processor", "Name");
            var cpuMake = new HardwareInformation()
            {
                MainValue = make,
                ShortName = "CPU",
                UnitSymbol = ""
            };
            return cpuMake;
        }

        private static HardwareInformation GetCpuTemperature()
        {
            var temp = WmiHelper.GetWmiValue<double>("MSAcpi_ThermalZoneTemperature", "CurrentTemperature", scope: @"root\WMI");
            var cpuTemp = new HardwareInformation()
            {
                ShortName = "CPU",
                MainValue = (temp - 2732) / 10.0,
                UnitSymbol = "°C"
            };
            return cpuTemp;
        }

        private static HardwareInformation GetCpuThreadCount()
        {
            var threadCount = WmiHelper.GetWmiValue<uint>("Win32_Processor", "ThreadCount");
            var cpuThreadCount = new HardwareInformation()
            {
            MainValue = (double)threadCount,
            ShortName = "CPU",
            UnitSymbol = (threadCount > 1) ? "Threads" : "Thread",
            };

            return cpuThreadCount;
        }

        private static IEnumerable<HardwareInformation> GetEachCpuUsage()
        {
            var wmiObject = new ManagementObjectSearcher("select * from Win32_PerfFormattedData_PerfOS_Processor");

            var allCpuUsage = wmiObject.Get()
                                   .Cast<ManagementObject>()
                                   .Select(mo => new HardwareInformation
                                   {
                                       ShortName = "CPU" + mo["Name"].ToString(),
                                       MainValue = Double.Parse(mo["PercentProcessorTime"].ToString()),
                                       UnitSymbol = "%"
                                   }
                                   )
                                   .ToList();

            return (allCpuUsage.Count() != 0) ? allCpuUsage :
                throw new ArgumentNullException("No cpu usage was found in ManagementObjectSearcher");
        }

        private static HardwareInformation GetGlobalCpuUsage()
        {
            var loadPercentage = WmiHelper.GetWmiValue<double>("Win32_Processor", "LoadPercentage");
            var cpuUsage = new HardwareInformation()
            {
                MainValue = loadPercentage,
                ShortName = "CPU",
                UnitSymbol = "%"
            };
            return cpuUsage;
        }

        private static HardwareInformation GetGlobalCpuUsageWithPerfCounter()
        {
            all_Cpu_Idle = (all_Cpu_Idle == null) ? new PerformanceCounter("Processor", "% Idle Time", "_Total") : all_Cpu_Idle;
            var cpuIdle = all_Cpu_Idle.NextValue();
            return new HardwareInformation()
            {
                MainValue = Math.Round(100.0 - cpuIdle, 2),
                ShortName = "CPU",
                UnitSymbol = "%"
            };
        }

        private static HardwareInformation GetRamMemoryUsage()
        {
            var totalMemSize = WmiHelper.GetWmiValue<double>("Win32_OperatingSystem", "TotalVisibleMemorySize");
            var freeMemSize = WmiHelper.GetWmiValue<double>("Win32_OperatingSystem", "FreePhysicalMemory");

            var ramUsage = new HardwareInformation()
            {
                MainValue = 100 * Math.Round(((totalMemSize - freeMemSize) / totalMemSize), 2),
                ShortName = "RAM",
                UnitSymbol = "%"
            };

            return (ramUsage != null) ? ramUsage :
            throw new ArgumentNullException("No memory was found in ManagementObjectSearcher"); ;
        }

        #endregion

        public override HardwareInformation GetValue(MonitoringTarget ressource)
        {
            switch (ressource)
            {
                case MonitoringTarget.CPU_Core_Count:
                    return GetCpuCoreCount();

                case MonitoringTarget.CPU_Clock_Speed:
                    return GetCpuClockSpeed();

                case MonitoringTarget.CPU_Make:
                    return GetCpuMake();

                case MonitoringTarget.CPU_Temp:
                    return GetCpuTemperature();

                case MonitoringTarget.CPU_Thread_Count:
                    return GetCpuThreadCount();
                    
                case MonitoringTarget.CPU_Load:
                    return GetGlobalCpuUsageWithPerfCounter();

                case MonitoringTarget.RAM_Usage:
                    return GetRamMemoryUsage();

                default:
                    throw new NotImplementedException($"Monitoring target '{ressource}' is not implemented for connector {nameof(WMI_Connector)}");
            }
        }
    }
}
