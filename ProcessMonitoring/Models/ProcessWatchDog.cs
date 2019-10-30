using Common.Helpers;
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

namespace ProcessMonitoring.Models
{
    public class ProcessWatchDog
    {
        private static CaptureFileWriterDevice _captureFileWriterReceived;
        private static CaptureFileWriterDevice _captureFileWriterSend;
        private CommandLineHelper _cmdHelper;

        private static ICaptureDevice _captureDevice;

        public static ProcessPacketInfo ProccessInfo;
        public delegate void PacketsExchanged();
        public event PacketsExchanged PacketsExchangedEvent;

        public ProcessWatchDog()
        {
            _cmdHelper = new CommandLineHelper();
        }

        private static void CaptureOutgoingPackets(string IP, int portID, ICaptureDevice device)
        {
            device.OnPacketArrival += new PacketArrivalEventHandler(OnPacketArrival_Send);
            int readTimeoutMilliseconds = 1000;
            device.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds);
            string filter = "src host " + IP + " and src port " + portID;
            device.Filter = filter;
            device.StartCapture();
        }

        private static void OnPacketArrival_Send(object sender, CaptureEventArgs e)
        {
            var len = e.Packet.Data.Length;
            ProccessInfo.NetSendBytes += len;
            _captureFileWriterSend.Write(e.Packet);
        }

        private static void CaptureIncomingPackets(string IP, int portID, ICaptureDevice device)
        {
            device.OnPacketArrival += new PacketArrivalEventHandler(OnPacketArrival_Recv);

            int readTimeoutMilliseconds = 1000;
            device.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds);

            string filter = "dst host " + IP + " and dst port " + portID;
            device.Filter = filter;
            device.StartCapture();

        }

        private static void OnPacketArrival_Recv(object sender, CaptureEventArgs e)
        {
            var len = e.Packet.Data.Length;
            ProccessInfo.NetRecvBytes += len;
            _captureFileWriterReceived.Write(e.Packet);
        }

        public KeyValuePair<int, IEnumerable<int>> GetOpenPortsForProcess(string appProcessName)
        {
            var all_related_pids = GetProcessesIdByName(appProcessName).Select(P => P.Id);
            var pid = all_related_pids.FirstOrDefault();
            ProccessInfo = new ProcessPacketInfo(pid);
            var network_status = _cmdHelper.ExecuteCommand("netstat -ano");
            var all_ports_for_process = GetOpenTCPandUDPportsForProcess(pid, network_status);
            return new KeyValuePair<int, IEnumerable<int>>(pid, all_ports_for_process);
        }

        private IEnumerable<Process> GetProcessesIdByName(string process_name)
        {
            Process[] allprocesses = Process.GetProcesses();
            return allprocesses.Where(P => P.ProcessName == process_name);
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

        public void InitializeWatchdog(int pid, IEnumerable<int> all_ports_for_process)
        {
            var localHostIpV4 = GetIpV4OfLocalHost();
            _captureDevice = CaptureDeviceList.Instance.Where(CD => !CD.Description.Contains("Atheros")).FirstOrDefault() ??
                                throw new ArgumentNullException("No capture device found");
            var fileId = DateTime.Now.Day.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Year.ToString();
            _captureFileWriterReceived = new CaptureFileWriterDevice($@"D:\Guillaume\Desktop\Internet Explorer\Logs\PacketCaptures\packets_RCV_{fileId}.pcap");
            _captureFileWriterSend = new CaptureFileWriterDevice($@"D:\Guillaume\Desktop\Internet Explorer\Logs\PacketCaptures\packets_SENT_{fileId}.pcap");

            foreach (var port in all_ports_for_process)
            {
                CaptureIncomingPackets(localHostIpV4.ToString(), port, _captureDevice);
                CaptureOutgoingPackets(localHostIpV4.ToString(), port, _captureDevice);
            }
        }

        public bool IsProcessCurrentlyRunning(string appProcessName)
        {
            var all_related_processes = GetProcessesIdByName(appProcessName);
            if (all_related_processes.Count() == 0) return false;
            return true;
        }

        public async void RefreshInfo()
        {
            ProccessInfo.NetRecvBytes = 0;
            ProccessInfo.NetSendBytes = 0;
            ProccessInfo.NetTotalBytes = 0;
            await Task.Delay(900);
            ProccessInfo.NetTotalBytes = ProccessInfo.NetRecvBytes + ProccessInfo.NetSendBytes;
            if (ProccessInfo.NetTotalBytes > 0) PacketsExchangedEvent();
        }

        public void StopCapturingPackets()
        {
            if (_captureDevice != null)
            {
                _captureDevice.OnPacketArrival -= OnPacketArrival_Send;
                _captureDevice.OnPacketArrival -= OnPacketArrival_Recv;
            }
        }
    }
}
