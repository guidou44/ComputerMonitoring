using SharpPcap;
using SharpPcap.LibPcap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProcessMonitoring.Models
{
    public class PacketCaptureProcessInfo
    {
        public PacketCaptureProcessInfo(Process process)
        {
            Process = process;
        }

        public PacketCaptureProcessInfo() { }

        public virtual Process Process { get; set; }

        public virtual string ProcessName 
        {
            get { return Process.ProcessName; }
        }

        public virtual int PID
        {
            get { return Process.Id; }            
        }

        public ICaptureFileWriter ReceivedCaptureFileWriter { get; set; }
        public ICaptureFileWriter SentCaptureFileWriter { get; set; }
        public long NetSendBytes { get; set; }
        public long NetRecvBytes { get; set; }
        public long NetTotalBytes { get; set; }
        public virtual IEnumerable<int> Ports { get; set; }
        public IPacketCaptureDevice CaptureDevice { get; set; }

        public virtual void SetOpenTCPandUDPportsForProcess(StreamReader network_Status, int pid)
        {
            if (Process == null)
                throw new ArgumentNullException("no process defined for capture info");

            ICollection<int> ports = new List<int>();

            Regex reg = new Regex("\\s+", RegexOptions.Compiled);
            string line = null;

            while ((line = network_Status.ReadLine()) != null)
            {
                line = line.Trim();
                string[] arr = reg.Replace(line, ",").Split(',');
                if (line.StartsWith("TCP", StringComparison.OrdinalIgnoreCase))
                {
                    int associatedPid = int.Parse(arr[4]);
                    if (associatedPid == pid)
                    {
                        string socket = arr[1];
                        int port_start_index = socket.LastIndexOf(':');
                        int port = int.Parse(socket.Substring(port_start_index + 1));
                        ports.Add(port);
                    }
                }
                else if (line.StartsWith("UDP", StringComparison.OrdinalIgnoreCase))
                {
                    int associatedPid = int.Parse(arr[3]);
                    if (associatedPid == pid)
                    {
                        string socket = arr[1];
                        int port_start_index = socket.LastIndexOf(':');
                        int port = int.Parse(socket.Substring(port_start_index + 1));
                        ports.Add(port);
                    }
                }
            }

            Ports = ports;
        }
    }
}
