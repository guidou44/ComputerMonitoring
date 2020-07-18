using Hardware.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hardware.Exceptions
{
    public class HardwareCommunicationException : Exception
    {
        public HardwareCommunicationException(MonitoringTarget monTarget) : base($"Can't reach {monTarget} on computer")
        { }

    }
}
