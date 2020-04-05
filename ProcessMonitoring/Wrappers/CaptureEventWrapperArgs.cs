using ProcessMonitoring.Models;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessMonitoring.Wrappers
{
    public class CaptureEventWrapperArgs : EventArgs
    {
        private CaptureEventArgs _eventArgs_Internal;
        public CaptureEventWrapperArgs(PacketWrapper packet, ICaptureDevice device)
        {
            _eventArgs_Internal = new CaptureEventArgs(packet, device);
        }

        public CaptureEventWrapperArgs(RawCapture packet, ICaptureDevice device)
        {
            _eventArgs_Internal = new CaptureEventArgs(packet, device);
        }

        public CaptureEventWrapperArgs() { }

        public static implicit operator CaptureEventArgs(CaptureEventWrapperArgs e)
        {
            return e._eventArgs_Internal;
        }

        public static implicit operator CaptureEventWrapperArgs(CaptureEventArgs e)
        {
            return new CaptureEventWrapperArgs(e.Packet, e.Device);
        }

        public virtual PacketWrapper Packet 
        {
            get { return new PacketWrapper(_eventArgs_Internal.Packet); }
        }

        public ICaptureDevice Device 
        {
            get { return _eventArgs_Internal.Device; }
        }
    }
}
