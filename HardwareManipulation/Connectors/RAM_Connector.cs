
using HardwareManipulation.Enums;
using HardwareManipulation.Helpers;
using HardwareManipulation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace HardwareManipulation.Connectors
{
    public class RAM_Connector : ConnectorBase
    {
        public static HardwdareInformation GetCurrentRamMemoryUsage()
        {
            var totalMemSize = WmiHelper.GetWmiValue<double>("Win32_OperatingSystem", "TotalVisibleMemorySize");
            var freeMemSize = WmiHelper.GetWmiValue<double>("Win32_OperatingSystem", "FreePhysicalMemory");

            var ramUsage = new HardwdareInformation()
            {
                Main_Value = Math.Round((totalMemSize - freeMemSize) / totalMemSize),
                ShortName = "RAM",
                UnitSymbol = "%"
            };

            return (ramUsage != null) ? ramUsage :
            throw new ArgumentNullException("No memory was found in ManagementObjectSearcher"); ;
        }

        public override HardwdareInformation GetValue(MonitoringTarget ressource)
        {
            switch (ressource)
            {
                case MonitoringTarget.RAM_Usage:
                    return GetCurrentRamMemoryUsage();
                default:
                    throw new NotImplementedException($"Computer ressource {ressource} is not implemented for connector {nameof(RAM_Connector)}");
            }
        }
    }
}
