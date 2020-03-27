using Common.Helpers;
using ProcessMonitoring.Models;
using SharpPcap;
using SharpPcap.LibPcap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessMonitoring
{
    public class ProcessWatchDog
    {
        #region Constructor

        private CommandLineHelper _cmdHelper;
        private const string _XML_CONFIG_PATH = @".\Configuration\WatchdogConfiguration.cfg";
        private IPAddress localHostIpV4;
        private ICollection<PacketCaptureProcessInfo> _packetCaptureProcessesInfo;

        public delegate void PacketsExchanged(PacketCaptureProcessInfo pcp);
        public event PacketsExchanged PacketsExchangedEvent;

        public ProcessWatchDog()
        {
            _cmdHelper = new CommandLineHelper();
            _packetCaptureProcessesInfo = new HashSet<PacketCaptureProcessInfo>();
            localHostIpV4 = GetIpV4OfLocalHost();
        }

        #endregion

        #region Public Methods

        public IEnumerable<string> GetInitialProcesses2Watch()
        {
            var watchdogInit = XmlHelper.DeserializeConfiguration<WatchdogInitialization>(_XML_CONFIG_PATH);
            return watchdogInit.InitialProcess2watchNames;
        }

        public IEnumerable<Process> GetProcessesByName(string process_name)
        {
            Process[] allprocesses = Process.GetProcesses();
            return allprocesses.Where(P => P.ProcessName == process_name);
        }

        public void SetProcessAndItsOpenPortsInfo(Process process)
        {
            var processCaptureInfo = new PacketCaptureProcessInfo(process);
            var network_status = _cmdHelper.ExecuteCommand("netstat -ano");
            processCaptureInfo.Ports = GetOpenTCPandUDPportsForProcess(process.Id, network_status);
            _packetCaptureProcessesInfo.Add(processCaptureInfo);
        }

        public void InitializeWatchdogForProcess(Process process)
        {
            SetProcessAndItsOpenPortsInfo(process);
            var processCaptureInfo = _packetCaptureProcessesInfo.LastOrDefault();
            var fileId = DateTime.Now.Day.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Year.ToString();
            var currentDirectory = Directory.GetCurrentDirectory().ToString();
            if (!Directory.Exists(Path.Combine(currentDirectory, "Packet_Captures"))) Directory.CreateDirectory(Path.Combine(currentDirectory, "Packet_Captures"));
            processCaptureInfo.ReceivedCaptureFileWriter = new CaptureFileWriterDevice(Path.Combine(currentDirectory, "Packet_Captures", $"{process.ProcessName}_packets_RCV_{fileId}.pcap"));
            processCaptureInfo.SentCaptureFileWriter = new CaptureFileWriterDevice(Path.Combine(currentDirectory, "Packet_Captures", $"{process.ProcessName}_packets_SENT_{fileId}.pcap"));
            processCaptureInfo.CaptureDevice = CaptureDeviceList.Instance.Where(CD => !CD.Description.Contains("Atheros")).FirstOrDefault() ??
                throw new ArgumentNullException("No capture device found");
            foreach (var port in processCaptureInfo.Ports)
            {
                CaptureIncomingPackets(localHostIpV4.ToString(), port, processCaptureInfo.CaptureDevice, processCaptureInfo.Process);
                CaptureOutgoingPackets(localHostIpV4.ToString(), port, processCaptureInfo.CaptureDevice, processCaptureInfo.Process);
            }
        }

        public bool IsProcessCurrentlyRunning(string appProcessName)
        {
            var all_related_processes = GetProcessesByName(appProcessName);
            if (all_related_processes.Count() == 0) return false;
            return true;
        }

        public void RefreshInfo()
        {
            foreach (var packetCaptureProcessInfo in _packetCaptureProcessesInfo)
            {
                packetCaptureProcessInfo.NetRecvBytes = 0;
                packetCaptureProcessInfo.NetSendBytes = 0;
                packetCaptureProcessInfo.NetTotalBytes = 0;
                packetCaptureProcessInfo.NetTotalBytes = packetCaptureProcessInfo.NetRecvBytes + packetCaptureProcessInfo.NetSendBytes;
            }
        }

        public void StopCapturingPackets(Process process)
        {
            var packetCaptureProcess = _packetCaptureProcessesInfo.Where(PCP => PCP.Process == process).SingleOrDefault();
            if (packetCaptureProcess.CaptureDevice != null)
            {
                packetCaptureProcess.CaptureDevice.OnPacketArrival -= (sender, e) => OnPacketArrival_Send(sender, e, packetCaptureProcess.Process);
                packetCaptureProcess.CaptureDevice.OnPacketArrival -= (sender, e) => OnPacketArrival_Recv(sender, e, packetCaptureProcess.Process);
            }
        }

        #endregion

        #region Private Methods

        private void CaptureOutgoingPackets(string IP, int portID, ICaptureDevice device, Process process)
        {
            device.OnPacketArrival += new PacketArrivalEventHandler((sender, e) => OnPacketArrival_Send(sender, e, process));
            int readTimeoutMilliseconds = 1000;
            device.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds);
            string filter = "src host " + IP + " and src port " + portID;
            device.Filter = filter;
            device.StartCapture();
        }

        private void OnPacketArrival_Send(object sender, CaptureEventArgs e, Process process)
        {
            var len = e.Packet.Data.Length;
            var guiltyProcess = _packetCaptureProcessesInfo.Where(PCP => PCP.Process.Id == process.Id).FirstOrDefault();
            guiltyProcess.NetSendBytes += len;
            guiltyProcess.SentCaptureFileWriter.Write(e.Packet);
            PacketsExchangedEvent?.Invoke(guiltyProcess);
        }

        private void CaptureIncomingPackets(string IP, int portID, ICaptureDevice device, Process process)
        {
            device.OnPacketArrival += new PacketArrivalEventHandler((sender, e) => OnPacketArrival_Recv(sender, e, process));
            int readTimeoutMilliseconds = 1000;
            device.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds);
            string filter = "dst host " + IP + " and dst port " + portID;
            device.Filter = filter;
            device.StartCapture();

        }

        private void OnPacketArrival_Recv(object sender, CaptureEventArgs e, Process process)
        {
            var len = e.Packet.Data.Length;
            var guiltyProcess = _packetCaptureProcessesInfo.Where(PCP => PCP.Process.Id == process.Id).FirstOrDefault();
            guiltyProcess.NetRecvBytes += len;
            guiltyProcess.ReceivedCaptureFileWriter.Write(e.Packet);
            PacketsExchangedEvent?.Invoke(guiltyProcess);
        }

        private ICollection<int> GetOpenTCPandUDPportsForProcess(int pid, StreamReader network_Status)
        {
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

            return ports;
        }

        private IPAddress GetIpV4OfLocalHost()
        {
            IPAddress[] addrList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            return addrList.Where(IP => IP.AddressFamily == AddressFamily.InterNetwork).FirstOrDefault();
        }

        #endregion
    }
}
