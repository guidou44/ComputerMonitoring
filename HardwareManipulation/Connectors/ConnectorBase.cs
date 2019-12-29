using HardwareManipulation.Enums;
using HardwareManipulation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareManipulation.Connectors
{
    public abstract class ConnectorBase
    {
        public abstract HardwdareInformation GetValue(MonitoringTarget ressource);
    }
}
