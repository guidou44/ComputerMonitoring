using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Common.Helpers;
using DesktopAssistant.BL;
using DesktopAssistant.BL.ProcessWatch;
using PacketDotNet;
using ProcessMonitoring.Models;
using ProcessMonitoring.Factory;
using SharpPcap;
using Timer = System.Timers.Timer;

namespace ProcessMonitoring
{
    public class ProcessWatcher : IProcessWatcher
    {
        private const string XmlConfigPath = @"..\..\Configuration\WatchdogConfiguration.cfg";
        private const string NotSupportedCaptureDevice = "Atheros";
        private const int ExecutionThreadTimeout = 2000;

        private readonly NetworkHelper _networkHelper;
        private readonly XmlHelper _xmlHelper;
        private readonly IPAddress _localHostIpV4;
        private readonly ICaptureDevice _captureDevice;
        private readonly ITimer _watchJobTimer;
        private readonly ICollection<ProcessWatch> _processWatches = new HashSet<ProcessWatch>();
        
        private List<int> _portsCurrentlyUnderWatch = new List<int>();

        private IPacketObserver _packetObserver;

        private PacketArrivalEventHandler _packetCaptureHandler;


        public ProcessWatcher(NetworkHelper networkHelper, XmlHelper xmlHelper, 
            ICaptureDeviceFactory captureDeviceDeviceFactory, ITimer watchJobTimer)
        {
            _networkHelper = networkHelper;
            _xmlHelper = xmlHelper;
            
            _watchJobTimer = watchJobTimer;

            _captureDevice = captureDeviceDeviceFactory.CreateInstance(NotSupportedCaptureDevice);
            _localHostIpV4 = GetIpV4OfLocalHost();
            BuildProcessWatches();
        }

        public IEnumerable<IProcessWatch> GetProcessUnderWatch()
        {
            return _processWatches;
        }

        public void RegisterPacketCaptureObserver(IPacketObserver packetObserver)
        {
            _packetObserver = packetObserver;
        }

        public void StartCapture()
        {
            _watchJobTimer.Init(3000);
            _watchJobTimer.AutoReset = false;
            _watchJobTimer.Enabled = true;
            _watchJobTimer.Elapsed += OnWatchJobTimerElapsed;
            _watchJobTimer.Start();
        }

        public void StopCapture()
        {
            StopCapture_Internal();
            _watchJobTimer.Stop();
            _watchJobTimer.Enabled = false;
            _watchJobTimer.Elapsed -= OnWatchJobTimerElapsed;
        }

        public void AddProcessToWatchList(string processName, bool withCapture)
        {
            BuildProcessWatch(processName, withCapture); 
        }

        public void RemoveProcessFromWatchList(string processName)
        {
            ProcessWatch processWatch = _processWatches.SingleOrDefault(pw => pw.ProcessName.Equals(processName));
            if (processWatch != null)
            {
                RemoveProcessWatchPorts(processWatch);
                _processWatches.Remove(processWatch);
            }
        }

        public void UpdateProcessCaptureInWatchList(string processName, bool doCapture)
        {
            ProcessWatch processWatch = _processWatches.SingleOrDefault(pw => pw.ProcessName.Equals(processName));
            if (processWatch != null)
            {                
                processWatch.DoCapture = doCapture;
                if (!doCapture)
                    RemoveProcessWatchPorts(processWatch);
            }

        }

        #region private methods

        private async void OnWatchJobTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _watchJobTimer.Stop();
            _watchJobTimer.Enabled = false;
            await Task.Run(CheckForWatchListUpdate);
            _watchJobTimer.Enabled = true;
            _watchJobTimer.Start();
        }
        
        private void CheckForWatchListUpdate()
        {
            lock (_processWatches)
            {
                foreach (ProcessWatch processWatch in _processWatches)
                {
                    
                    processWatch.Process = processWatch.Process ?? GetFirstProcessWithName(processWatch.ProcessName);
                    if (!processWatch.IsRunning)
                        continue;

                    processWatch.Ports = _networkHelper.GetOpenTcpAndUdpPortsForProcess(processWatch.Process);
                    IEnumerable<int> notWatchedPorts = processWatch.Ports.Except(_portsCurrentlyUnderWatch);

                    if (notWatchedPorts.Any())
                    {
                        _portsCurrentlyUnderWatch.AddRange(notWatchedPorts);
                        StopCapture_Internal();
                        BuildDeviceFilter();
                        StartCapture_Internal();
                    }
                }
            }
        }

        private void RemoveProcessWatchPorts(ProcessWatch processWatch)
        {
            _portsCurrentlyUnderWatch = _portsCurrentlyUnderWatch
                .Where(p => !processWatch.Ports.Contains(p)).ToList();
        }

        private void StartCapture_Internal()
        {
            if (!_portsCurrentlyUnderWatch.Any())
                return;
            
            _packetCaptureHandler = (sender, e) =>
            {
                PacketData packetData = BuildPacketData(e.Packet);
                _packetObserver?.OnPacketCapture(packetData);
            };
            _captureDevice.OnPacketArrival += _packetCaptureHandler;
        }

        private PacketData BuildPacketData(RawCapture rawCapture)
        {
            Packet packet = Packet.ParsePacket(rawCapture.LinkLayerType, rawCapture.Data);
            
            return new PacketData()
            {
                Time = rawCapture.Timeval.Date,
                Length = rawCapture.Data.Length,
                RawData =  rawCapture.Data,
                SourceAddress = GetSourceAddressOrPort(packet),
                DestinationAddress = GetDestinationAddressOrPort(packet),
                Summary = rawCapture.ToString()
            };
        }

        private string GetSourceAddressOrPort(Packet packet)
        {
            TcpPacket tcp = packet.Extract<TcpPacket>();
            UdpPacket udp = packet.Extract<UdpPacket>();
            IPPacket ip = packet.Extract<IPPacket>();

            if (ip != null)
                return ip.SourceAddress.ToString();
            if (tcp != null)
                return tcp.SourcePort.ToString();
            return udp != null ? udp.SourcePort.ToString() : "";
        }
        
        private string GetDestinationAddressOrPort(Packet packet)
        {
            TcpPacket tcp = packet.Extract<TcpPacket>();
            UdpPacket udp = packet.Extract<UdpPacket>();
            IPPacket ip = packet.Extract<IPPacket>();

            if (ip != null)
                return ip.DestinationAddress.ToString();
            if (tcp != null)
                return tcp.DestinationPort.ToString();
            return udp != null ? udp.DestinationPort.ToString() : "";
        }

        private void StopCapture_Internal()
        {
            _captureDevice.OnPacketArrival -= _packetCaptureHandler;
            _captureDevice.Close();
        }

        private void BuildDeviceFilter()
        {
            if (!_portsCurrentlyUnderWatch.Any())
                return;
            
            string baseFilter = $"src host {_localHostIpV4} and port {_portsCurrentlyUnderWatch.First()}";
            foreach (int port in _portsCurrentlyUnderWatch.Skip(1))
            {
                baseFilter += $" or port {port}";
            }

            _captureDevice.Filter = baseFilter;
        }
        
        private Process GetFirstProcessWithName(string processName)
        {
            Process[] allProcess = Process.GetProcesses();
            return allProcess.FirstOrDefault(p => p.ProcessName == processName);
        }
        
        private IPAddress GetIpV4OfLocalHost()
        {
            IPAddress[] addrList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            return addrList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        }

        private void BuildProcessWatches()
        {
            WatchdogInitialization watchdogInit = _xmlHelper.DeserializeConfiguration<WatchdogInitialization>(XmlConfigPath);
            foreach (string processName in watchdogInit.InitialProcess2WatchNames)
            {
                BuildProcessWatch(processName, true);
            }
        }

        private void BuildProcessWatch(string processName, bool withCapture)
        {
            Process process = GetFirstProcessWithName(processName);
            ProcessWatch processWatch = new ProcessWatch(processName, withCapture, process);
            _processWatches.Add(processWatch);
        }
        
        
        
        #endregion
        
    }
}