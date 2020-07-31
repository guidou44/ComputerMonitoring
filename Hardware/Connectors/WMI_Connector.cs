using System;
using DesktopAssistant.BL.Hardware;
using Hardware.Components;
using Hardware.Helpers;
using Hardware.Models;

namespace Hardware.Connectors
{
    public class WMI_Connector : ConnectorBase
    {
        private IPerformanceCounter all_Cpu_Idle;
        private WmiHelper wmiHelper;

        public WMI_Connector(WmiHelper wmiHelper, IPerformanceCounter all_Cpu_Idle)
        {
            this.wmiHelper = wmiHelper;
            this.all_Cpu_Idle = all_Cpu_Idle;
        }

        private HardwareInformation GetCpuCoreCount()
        {
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
            var threadCount = wmiHelper.GetWmiValue<uint>("Win32_Processor", "ThreadCount");
            var cpuThreadCount = new HardwareInformation()
            {
                MainValue = (double)threadCount,
                ShortName = "CPU",
                UnitSymbol = (threadCount > 1) ? "Threads" : "Thread",
            };

            return cpuThreadCount;
        }

        private HardwareInformation GetGlobalCpuUsageWithPerfCounter()
        {
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
            var totalMemSize = wmiHelper.GetWmiValue<double>("Win32_OperatingSystem", "TotalVisibleMemorySize");
            var freeMemSize = wmiHelper.GetWmiValue<double>("Win32_OperatingSystem", "FreePhysicalMemory");

            var ramUsage = new HardwareInformation()
            {
                MainValue = 100 * Math.Round(((totalMemSize - freeMemSize) / totalMemSize), 2),
                ShortName = "RAM",
                UnitSymbol = "%"
            };

            return ramUsage;
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