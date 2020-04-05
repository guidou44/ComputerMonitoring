using PacketDotNet;
using ProcessMonitoring.Models;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessMonitoring.Wrappers
{
    public class PacketWrapper
    {
        private RawCapture packetInternal;
        public PacketWrapper(RawCapture capture)
        {
            packetInternal = capture;
        }

        public static implicit operator RawCapture(PacketWrapper wrapper)
        {
            return wrapper.packetInternal;
        }

        public static implicit operator PacketWrapper(RawCapture capture)
        {
            return new PacketWrapper(capture);
        }

        public PacketWrapper() { }

        public virtual byte[] Data
        {
            get { return packetInternal.Data; }
        }
    }
}
