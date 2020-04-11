using Common.Helpers;
using ProcessMonitoring.Models;
using ProcessMonitoring.Wrappers;
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
        private const string _XML_CONFIG_PATH = @".\Configuration\WatchdogConfiguration.cfg";

        private CommandLineHelper _cmdHelper;
        private XmlHelper _xmlHelper;
        private IPAddress localHostIpV4;        
        private ICaptureFactory<IPacketCaptureDevice> captureDeviceFactory;
        private ICaptureFactory<ICaptureFileWriter> captureWriterFactory;

        protected ICollection<PacketCaptureProcessInfo> _packetCaptureProcessesInfo;

        public delegate void PacketsExchanged(PacketCaptureProcessInfo pcp);
        public virtual event PacketsExchanged PacketsExchangedEvent;

        public ProcessWatchDog(CommandLineHelper cmdHelper, XmlHelper xmlHelper, 
            ICaptureFactory<IPacketCaptureDevice> captureDeviceFactory, 
            ICaptureFactory<ICaptureFileWriter> captureWriterFactory)
        {
            _cmdHelper = cmdHelper;
            _xmlHelper = xmlHelper;
            this.captureDeviceFactory = captureDeviceFactory;
            this.captureWriterFactory = captureWriterFactory;
            _packetCaptureProcessesInfo = new HashSet<PacketCaptureProcessInfo>();
            localHostIpV4 = GetIpV4OfLocalHost();
        }

        public ProcessWatchDog() { }

        #region Public Methods

        public virtual IEnumerable<string> GetInitialProcesses2Watch()
        {
            var watchdogInit = _xmlHelper.DeserializeConfiguration<WatchdogInitialization>(_XML_CONFIG_PATH);
            return watchdogInit.InitialProcess2watchNames;
        }

        public virtual IEnumerable<Process> GetProcessesByName(string process_name)
        {
            Process[] allprocesses = Process.GetProcesses();
            return allprocesses.Where(P => P.ProcessName == process_name);
        }

        private void SetProcessAndItsOpenPortsInfo(PacketCaptureProcessInfo captureInfo)
        {
            var network_status = _cmdHelper.ExecuteCommand("netstat -ano");
            captureInfo.SetOpenTCPandUDPportsForProcess(network_status, captureInfo.PID);  
            _packetCaptureProcessesInfo.Add(captureInfo);
        }

        public virtual void InitializeWatchdogForProcess(PacketCaptureProcessInfo processCaptureInfo)
        {
            SetProcessAndItsOpenPortsInfo(processCaptureInfo);
            string fileId = DateTime.Now.Day.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Year.ToString();

            string currentDirectory = Directory.GetCurrentDirectory().ToString();
            if (!Directory.Exists(Path.Combine(currentDirectory, "Packet_Captures"))) Directory.CreateDirectory(Path.Combine(currentDirectory, "Packet_Captures"));

            processCaptureInfo.ReceivedCaptureFileWriter = captureWriterFactory.CreateInstance(Path.Combine(currentDirectory, "Packet_Captures", $"{processCaptureInfo.ProcessName}_packets_RCV_{fileId}.pcap"));
            processCaptureInfo.SentCaptureFileWriter = captureWriterFactory.CreateInstance(Path.Combine(currentDirectory, "Packet_Captures", $"{processCaptureInfo.ProcessName}_packets_SENT_{fileId}.pcap"));
            processCaptureInfo.CaptureDevice = captureDeviceFactory.CreateInstance("Atheros");

            foreach (var port in processCaptureInfo.Ports)
            {
                CaptureIncomingPackets(localHostIpV4.ToString(), port, processCaptureInfo.CaptureDevice, processCaptureInfo);
                CaptureOutgoingPackets(localHostIpV4.ToString(), port, processCaptureInfo.CaptureDevice, processCaptureInfo);
            }
        }

        public virtual bool IsProcessCurrentlyRunning(string appProcessName)
        {
            var all_related_processes = GetProcessesByName(appProcessName);
            if (all_related_processes.Count() == 0) return false;
            return true;
        }

        public virtual void RefreshInfo()
        {
            foreach (var packetCaptureProcessInfo in _packetCaptureProcessesInfo)
            {
                packetCaptureProcessInfo.NetRecvBytes = 0;
                packetCaptureProcessInfo.NetSendBytes = 0;
                packetCaptureProcessInfo.NetTotalBytes = 0;
                packetCaptureProcessInfo.NetTotalBytes = packetCaptureProcessInfo.NetRecvBytes + packetCaptureProcessInfo.NetSendBytes;
            }
        }

        #endregion

        #region Private Methods

        private void CaptureOutgoingPackets(string IP, int portID, IPacketCaptureDevice device, PacketCaptureProcessInfo captureInfo)
        {
            device.OnPacketArrival += new PacketCaptureEventHandlerWrapper((sender, e) => OnPacketArrival_Send(sender, e, captureInfo));
            device.Open(readTimeOutMilliseconds:1000);
            device.Filter = "src host " + IP + " and src port " + portID;
            device.StartCapture();
        }

        private void OnPacketArrival_Send(object sender, CaptureEventWrapperArgs e, PacketCaptureProcessInfo captureInfo)
        {
            var len = e.Packet.Data.Length;
            captureInfo.NetSendBytes += len;
            captureInfo.SentCaptureFileWriter.Write(e);
            PacketsExchangedEvent?.Invoke(captureInfo);
        }

        private void CaptureIncomingPackets(string IP, int portID, IPacketCaptureDevice device, PacketCaptureProcessInfo captureInfo)
        {
            device.OnPacketArrival += new PacketCaptureEventHandlerWrapper((sender, e) => OnPacketArrival_Recv(sender, e, captureInfo));
            device.Open(readTimeOutMilliseconds: 1000);
            device.Filter = "dst host " + IP + " and dst port " + portID;
            device.StartCapture();
        }

        private void OnPacketArrival_Recv(object sender, CaptureEventWrapperArgs e, PacketCaptureProcessInfo captureInfo)
        {
            var len = e.Packet.Data.Length;
            captureInfo.NetRecvBytes += len;
            captureInfo.ReceivedCaptureFileWriter.Write(e);
            PacketsExchangedEvent?.Invoke(captureInfo);
        }

        private IPAddress GetIpV4OfLocalHost()
        {
            IPAddress[] addrList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            return addrList.Where(IP => IP.AddressFamily == AddressFamily.InterNetwork).FirstOrDefault();
        }

        #endregion
    }
}
