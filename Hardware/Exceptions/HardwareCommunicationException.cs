using System;
using DesktopAssistant.BL.Hardware;

namespace Hardware.Exceptions
{
    public class HardwareCommunicationException : Exception
    {
        public HardwareCommunicationException(MonitoringTarget monTarget) : 
            base($"Can't reach {monTarget} on computer")
        { }
        
        public HardwareCommunicationException(string message) : 
            base(nameof(HardwareCommunicationException) + ":\n" + message)
        { }
    }
}
