using SharpPcap;
using SharpPcap.LibPcap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessMonitoring.Models
{
    public class PacketCaptureProcessInfo
    {
        public PacketCaptureProcessInfo(Process process)
        {
            Process = process;
        }
        public Process Process { get; set; }
        public CaptureFileWriterDevice ReceivedCaptureFileWriter { get; set; }
        public CaptureFileWriterDevice SentCaptureFileWriter { get; set; }
        public long NetSendBytes { get; set; }
        public long NetRecvBytes { get; set; }
        public long NetTotalBytes { get; set; }
        public IEnumerable<int> Ports { get; set; }
        public ICaptureDevice CaptureDevice { get; set; }
    }
}
