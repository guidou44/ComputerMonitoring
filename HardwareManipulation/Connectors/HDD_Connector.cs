using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HardwareManipulation.Enums;
using HardwareManipulation.Models;

namespace HardwareManipulation.Connectors
{
    public class HDD_Connector : ConnectorBase
    {
        public override HardwareInformation GetValue(MonitoringTarget ressource)
        {
            switch (ressource)
            {
                case MonitoringTarget.HDD_Used_Space:
                    break;
                case MonitoringTarget.SSD_Used_Space:
                    break;

                default:
                    break;
            }
            return new HardwareInformation();
        }
    }
}
