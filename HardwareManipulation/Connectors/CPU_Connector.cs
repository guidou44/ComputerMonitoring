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
            var wmiObject = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature");
            var cpuUsage = wmiObject.Get()
                                   .Cast<ManagementObject>()
                                   .Select(mo => new HardwdareInformation
                                   {
                                       ShortName = "CPU",
                                       Main_Value = (Double.Parse(mo["CurrentTemperature"].ToString()) - 2732) / 10.0,
                                       UnitSymbol = "°C"
                                   }
                                   ).FirstOrDefault();

            return cpuUsage;
        }

        public static HardwdareInformation GetCpuThreadCount()
        {
            var wmiObject = new ManagementObjectSearcher("select * from Win32_Processor");

            var cpuUsage = wmiObject.Get()
                       .Cast<ManagementObject>()
                       .Select(mo => new HardwdareInformation
                       {
                           Main_Value = UInt32.Parse(mo["ThreadCount"].ToString()),
                           ShortName = "CPU",
                           UnitSymbol = "Threads"
                       }
                       )
                       .FirstOrDefault();

            return (cpuUsage != null) ? cpuUsage :
                throw new ArgumentNullException("No cpu usage was found in ManagementObjectSearcher");
        }

        public static HardwdareInformation GetCurrentGlobalCpuUsage()
        {
            var wmiObject = new ManagementObjectSearcher("select * from Win32_Processor");

            var cpuUsage = wmiObject.Get()
                                   .Cast<ManagementObject>()
                                   .Select(mo => new HardwdareInformation
                                   {
                                       Main_Value = Double.Parse(mo["LoadPercentage"].ToString()),
                                       ShortName = "CPU",
                                       UnitSymbol = "%"
                                       //Current_ClockSpeed = 0.001f * UInt32.Parse(mo["CurrentClockSpeed"].ToString()),
                                       //Number_of_cores = UInt32.Parse(mo["NumberOfCores"].ToString()),
                                       //Thread_count = UInt32.Parse(mo["ThreadCount"].ToString())
                                   }
                                   )
                                   .FirstOrDefault();

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

        #region Private Methods

        private static T GetWmiValue<T>(string wmiPath, string wmiKey)
        {
            var wmiObject = new ManagementObjectSearcher($"select * from {wmiPath}");
            try
            {
                var value = wmiObject.Get().Cast<ManagementObject>()
                                     .Select(mo => mo[wmiKey].ToString()).FirstOrDefault();
                if (value == null) throw new ArgumentException($"Could not find wmiObject for key {wmiKey}");
                var converter = TypeDescriptor.GetConverter(typeof(T));
                return (converter != null) ? (T)converter.ConvertFromString(value) :
                    throw new ArgumentException($"Cannot convert tpye {typeof(T).Name} to string.");
            }
            catch (Exception e)
            {
                //Reporter.LogException(e);
                throw;
            }
        }

        #endregion
    }
}
