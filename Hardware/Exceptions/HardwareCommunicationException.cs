using System;
using DesktopAssistant.BL.Hardware;

namespace Hardware.Exceptions
{
    public class HardwareCommunicationException : Exception
    {
        public HardwareCommunicationException(MonitoringTarget monTarget) : base($"Can't reach {monTarget} on computer")
        { }
    }
}
