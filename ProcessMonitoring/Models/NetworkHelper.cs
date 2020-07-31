using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Common.Helpers;

namespace ProcessMonitoring.Models
{
    public class NetworkHelper
    {
        private readonly CommandLineHelper _cmdHelper;
        
        public NetworkHelper(CommandLineHelper cmdHelper)
        {
            _cmdHelper = cmdHelper;
        }

        public virtual IEnumerable<int> GetOpenTcpAndUdpPortsForProcess(Process process, int? passedPid = null)
        {
            if (process == null)
                throw new ArgumentNullException("no process defined for capture info");

            ICollection<int> ports = new List<int>();
            int pid = passedPid ?? process.Id;

            Regex reg = new Regex("\\s+", RegexOptions.Compiled);
            string line = null;
            StreamReader networkStatus = _cmdHelper.ExecuteCommand("netstat -ano");
            
            while ((line = networkStatus.ReadLine()) != null)
            {
                line = line.Trim();
                string[] cmdLineArray = reg.Replace(line, ",").Split(',');
                
                if (line.StartsWith("TCP", StringComparison.OrdinalIgnoreCase))
                {
                    int associatedPid = int.Parse(cmdLineArray[4]);
                    ParseForOpenPorts(cmdLineArray, ports, associatedPid, pid);
                }
                else if (line.StartsWith("UDP", StringComparison.OrdinalIgnoreCase))
                {
                    int associatedPid = int.Parse(cmdLineArray[3]);
                    ParseForOpenPorts(cmdLineArray, ports, associatedPid, pid);
                }
            }

            return ports;
        }
        
        private static void ParseForOpenPorts(string[] cmdLineArray, ICollection<int> ports, int lineAssociatedPid, int pid)
        {
            if (lineAssociatedPid == pid)
            {
                string socket = cmdLineArray[1];
                int portStartIndex = socket.LastIndexOf(':');
                int port = int.Parse(socket.Substring(portStartIndex + 1));
                ports.Add(port);
            }
        }
    }
}