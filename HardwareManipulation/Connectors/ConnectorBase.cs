using HardwareAccess.Enums;
using HardwareAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareAccess.Connectors
{
    public abstract class ConnectorBase
    {
        public abstract HardwareInformation GetValue(MonitoringTarget resource);
    }
}
