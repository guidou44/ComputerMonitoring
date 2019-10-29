using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessMonitoring.Models
{
    public class ProcessPacketInfo
    {
        public ProcessPacketInfo(int pid)
        {
            ProcessID = pid;
        }

        public int ProcessID { get; set; }
        public long NetSendBytes { get; set; }
        public long NetRecvBytes { get; set; }
        public long NetTotalBytes { get; set; }

    }
}
