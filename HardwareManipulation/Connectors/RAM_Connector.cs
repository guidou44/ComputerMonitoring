
using HardwareManipulation.Enums;
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
        public static HardwareUsageBase GetCurrentRamMemoryUsage()
        {
            var wmiObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");

            var ramUsage = wmiObject.Get().Cast<ManagementObject>().Select(mo => new RamUsage{
                Main_Value = Math.Round((Double.Parse(mo["TotalVisibleMemorySize"].ToString()) - Double.Parse(mo["FreePhysicalMemory"].ToString()))
                        / Double.Parse(mo["TotalVisibleMemorySize"].ToString()) * 100, 2)
            }).FirstOrDefault();

            return (ramUsage != null) ? ramUsage :
            throw new ArgumentNullException("No memory was found in ManagementObjectSearcher"); ;
        }

        public override HardwareUsageBase GetValue(RessourceName ressource)
        {
            switch (ressource)
            {
                case RessourceName.RAM_Usage:
                    return GetCurrentRamMemoryUsage();
                default:
                    throw new NotImplementedException($"Computer ressource {ressource} is not implemented for connector {nameof(RAM_Connector)}");
            }
        }
    }
}
