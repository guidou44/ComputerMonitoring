using Common.Helpers;
using HardwareAccess.Enums;
using HardwareAccess.Helpers;
using HardwareAccess.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HardwareAccess.Connectors
{
    public class WMI_Connector : ConnectorBase
    {        
        private PerformanceCounter all_Cpu_Idle;
        private WmiHelper wmiHelper;

        private HardwareInformation GetCpuCoreCount()
        {
            if (wmiHelper == null) wmiHelper = new WmiHelper();
            var numOfCore = wmiHelper.GetWmiValue<uint>("Win32_Processor", "NumberOfCores");
            var cpuCoreCount = new HardwareInformation()
            {
                MainValue = (double) numOfCore,
                ShortName = "CPU",
                UnitSymbol = (numOfCore > 1) ? "Cores" : "Core",
            };
            return cpuCoreCount;
        }

        private HardwareInformation GetCpuClockSpeed()
        {
            if (wmiHelper == null) wmiHelper = new WmiHelper();
            var clockSpeed = wmiHelper.GetWmiValue<uint>("Win32_Processor", "CurrentClockSpeed");
            var cpuClockSpeed = new HardwareInformation()
            {
                MainValue = (double) (clockSpeed * 001f)/1000,
                ShortName = "CPU",
                UnitSymbol = "kHz",
            };
            return cpuClockSpeed;
        }

        private HardwareInformation GetCpuMake()
        {
            if (wmiHelper == null) wmiHelper = new WmiHelper();
            var make = wmiHelper.GetWmiValue<string>("Win32_Processor", "Name");
            var cpuMake = new HardwareInformation()
            {
                MainValue = make,
                ShortName = "CPU",
                UnitSymbol = ""
            };
            return cpuMake;
        }

        private HardwareInformation GetCpuTemperature()
        {
            if (wmiHelper == null) wmiHelper = new WmiHelper();
            var temp = wmiHelper.GetWmiValue<double>("MSAcpi_ThermalZoneTemperature", "CurrentTemperature", scope: @"root\WMI");
            var cpuTemp = new HardwareInformation()
            {
                ShortName = "CPU",
                MainValue = (temp - 2732) / 10.0,
                UnitSymbol = "°C"
            };
            return cpuTemp;
        }

        private HardwareInformation GetCpuThreadCount()
        {
            if (wmiHelper == null) wmiHelper = new WmiHelper();
            var threadCount = wmiHelper.GetWmiValue<uint>("Win32_Processor", "ThreadCount");
            var cpuThreadCount = new HardwareInformation()
            {
            MainValue = (double)threadCount,
            ShortName = "CPU",
            UnitSymbol = (threadCount > 1) ? "Threads" : "Thread",
            };

            return cpuThreadCount;
        }

        private IEnumerable<HardwareInformation> GetEachCpuUsage()
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

        private HardwareInformation GetGlobalCpuUsage()
        {
            if (wmiHelper == null) wmiHelper = new WmiHelper();
            var loadPercentage = wmiHelper.GetWmiValue<double>("Win32_Processor", "LoadPercentage");
            var cpuUsage = new HardwareInformation()
            {
                MainValue = loadPercentage,
                ShortName = "CPU",
                UnitSymbol = "%"
            };
            return cpuUsage;
        }

        private HardwareInformation GetGlobalCpuUsageWithPerfCounter()
        {
            if (wmiHelper == null) wmiHelper = new WmiHelper();
            all_Cpu_Idle = (all_Cpu_Idle == null) ? new PerformanceCounter("Processor", "% Idle Time", "_Total") : all_Cpu_Idle;
            var cpuIdle = all_Cpu_Idle.NextValue();
            return new HardwareInformation()
            {
                MainValue = Math.Round(100.0 - cpuIdle, 2),
                ShortName = "CPU",
                UnitSymbol = "%"
            };
        }

        private HardwareInformation GetRamMemoryUsage()
        {
            if (wmiHelper == null) wmiHelper = new WmiHelper();
            var totalMemSize = wmiHelper.GetWmiValue<double>("Win32_OperatingSystem", "TotalVisibleMemorySize");
            var freeMemSize = wmiHelper.GetWmiValue<double>("Win32_OperatingSystem", "FreePhysicalMemory");

            var ramUsage = new HardwareInformation()
            {
                MainValue = 100 * Math.Round(((totalMemSize - freeMemSize) / totalMemSize), 2),
                ShortName = "RAM",
                UnitSymbol = "%"
            };

            return (ramUsage != null) ? ramUsage :
            throw new ArgumentNullException("No memory was found in ManagementObjectSearcher"); ;
        }

        public override HardwareInformation GetValue(MonitoringTarget resource)
        {
            switch (resource)
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
                    throw new NotImplementedException($"Monitoring target '{resource}' is not implemented for connector {nameof(WMI_Connector)}");
            }
        }
    }
}
