using ProcessMonitoring.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessMonitoring.Models
{
    public interface IPacketCaptureDevice
    {
        event PacketCaptureEventHandlerWrapper OnPacketArrival;

        void Open(int readTimeOutMilliseconds);
        void StartCapture();

        string Filter { get; set; }
    }
}
