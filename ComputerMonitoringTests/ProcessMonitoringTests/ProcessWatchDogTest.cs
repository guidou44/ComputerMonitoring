using Common.Helpers;
using ComputerMonitoringTests.ProcessMonitoringTests.Helpers;
using Moq;
using ProcessMonitoring;
using ProcessMonitoring.Models;
using ProcessMonitoring.Wrappers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ComputerMonitoringTests.ProcessMonitoringTests
{
    public class ProcessWatchDogTest
    {
        private const string INITIAL_TARGET = "chrome";

        private string _packetExchanged = "";
        private bool? _handlerRegistered = null;
        private bool _deviceIsOpen = false;
        private byte[] _captureData = new byte[] { 1, 2, 3 };
        private bool _exchangeReported = false;
        private string _reportedName = "";

        [Fact]
        public void GivenConfigFile_WhenGettingInitialProcessToWatch_ThenItReturnsProper()
        {
            ProcessWatchDog watchdog = GivenWatchdog();

            IEnumerable<string> initialTargets = watchdog.GetInitialProcesses2Watch();

            Assert.True(initialTargets.Count() == 1);
            Assert.Contains(INITIAL_TARGET, initialTargets);
        }

        [Fact]
        public void GivenRunningProcess_WhenGetProcessByName_ThenItReturnsProperProcesses()
        {
            ProcessWatchDog watchdog = GivenWatchdog();
            Process runningProcess = WatchDogTestHelper.GivenFirstRunningProcess();

            IEnumerable<Process> processesFound = watchdog.GetProcessesByName(runningProcess.ProcessName);

            Assert.All(processesFound, p => Assert.True(p.ProcessName == runningProcess.ProcessName));
        }

        [Fact]
        public void GivenRunningProcess_ProcessIsRunning()
        {
            ProcessWatchDog watchdog = GivenWatchdog();
            Process runningProcess = WatchDogTestHelper.GivenFirstRunningProcess();

            Assert.True(watchdog.IsProcessCurrentlyRunning(runningProcess.ProcessName)) ;
        }

        [Fact]
        public void GivenNotRunningProcess_ProcessIsNotRunning()
        {
            ProcessWatchDog watchdog = GivenWatchdog();

            Assert.False(watchdog.IsProcessCurrentlyRunning("NOT_RUNNING_PROCESS"));
        }

        [Fact]
        public void GivenNotExistingCaptureDiectory_WhenInitializing_ItCreatesDirectory()
        {
            const string direcotryPath = @".\Packet_Captures";
            if (Directory.Exists(direcotryPath)) Directory.Delete(direcotryPath, true);
            ProcessWatchDog watchdog = GivenWatchdog();
            PacketCaptureProcessInfo captureInfo = GivenValidPacketCaptureInfo();

            watchdog.InitializeWatchdogForProcess(captureInfo);

            Assert.True(Directory.Exists(direcotryPath)) ;

            Directory.Delete(direcotryPath, true);
        }

        [Fact]
        public void GivenProcessWithBytes_WhenRefreshInfo_AllBytesAreResetToZero()
        {
            ProcessWatchDog watchdog = GivenWatchdog();
            PacketCaptureProcessInfo captureInfo = GivenValidPacketCaptureInfo();
            captureInfo.NetRecvBytes = 3;
            captureInfo.NetSendBytes = 4;
            captureInfo.NetTotalBytes = 5;

            watchdog.InitializeWatchdogForProcess(captureInfo);
            watchdog.RefreshInfo();

            Assert.Equal(0, captureInfo.NetRecvBytes);
            Assert.Equal(0, captureInfo.NetSendBytes);
            Assert.Equal(0, captureInfo.NetTotalBytes);
        }

        [Fact]
        public void GivenNotInitializedProcess_WhenInit_ThenItOpensCaptureDevice()
        {
            ProcessWatchDog subjectWatchdog = GivenWatchdog();
            PacketCaptureProcessInfo captureInfo = GivenValidPacketCaptureInfo();

            subjectWatchdog.InitializeWatchdogForProcess(captureInfo);

            Assert.True(_deviceIsOpen);
        }

        [Fact]
        public void GivenExistingProcessUnderWatch_WhenPacketExchange_ThenThenProperPacketExchangeIsrecorded()
        {
            _packetExchanged = "";
            ProcessWatchDog subjectWatchdog = GivenWatchdog();
            PacketCaptureProcessInfo captureInfo = GivenInvalidPacketCaptureInfo();            

            Assert.Equal(0, captureInfo.NetRecvBytes);
            Assert.Equal(0, captureInfo.NetSendBytes);
            Assert.True(string.IsNullOrEmpty(_packetExchanged));

            subjectWatchdog.InitializeWatchdogForProcess(captureInfo);

            Assert.Equal(2 * _captureData.Length, captureInfo.NetRecvBytes);
            Assert.Equal(_captureData.Length, captureInfo.NetSendBytes);
            Assert.False(string.IsNullOrEmpty(_packetExchanged));
        }

        [Fact]
        public void GivenNotInitProcess_WhenInit_ThenHandlerIsRegistered()
        {
            _handlerRegistered = null;
            ProcessWatchDog subjectWatchdog = GivenWatchdog();
            PacketCaptureProcessInfo captureInfo = GivenValidPacketCaptureInfo();

            subjectWatchdog.InitializeWatchdogForProcess(captureInfo);

            Assert.True(_handlerRegistered);
        }

        [Fact]
        public void GivenProcessUnderWatch_WhenPacketExchanged_ThenItReportsExchange()
        {
            _exchangeReported = false;
            _reportedName = "";
            ProcessWatchDog subjectWatchdog = GivenWatchdog();
            PacketCaptureProcessInfo captureInfo = GivenInvalidPacketCaptureInfo();
            subjectWatchdog.PacketsExchangedEvent += OnPacketExchange;

            subjectWatchdog.InitializeWatchdogForProcess(captureInfo);

            Assert.True(_exchangeReported);
            Assert.Equal(captureInfo.ProcessName, _reportedName);
        }

        [Fact]
        public void GivenProcessNoMoreUnderWatch_WhenPacketExchanged_ThenItDoesNotReportExchange()
        {
            _exchangeReported = false;
            _reportedName = "";
            ProcessWatchDog subjectWatchdog = GivenWatchdog();
            PacketCaptureProcessInfo captureInfo = GivenInvalidPacketCaptureInfo();
            subjectWatchdog.PacketsExchangedEvent += OnPacketExchange;
            subjectWatchdog.PacketsExchangedEvent -= OnPacketExchange;

            subjectWatchdog.InitializeWatchdogForProcess(captureInfo);

            Assert.False(_exchangeReported);
            Assert.NotEqual(captureInfo.ProcessName, _reportedName);
        }

        #region Private methods

        private void OnPacketExchange(PacketCaptureProcessInfo info)
        {
            _exchangeReported = true;
            _reportedName = info.ProcessName;
        }

        private ProcessWatchDog GivenWatchdog()
        {
            XmlHelper xmlHelper = GivenXmlHelper();
            CommandLineHelper cmdHelper = WatchDogTestHelper.GivenCommandLineHelper();
            ICaptureFactory<ICaptureFileWriter> fileWriterFac = GivenFileWriterFactory();
            ICaptureFactory<IPacketCaptureDevice> captureDeviceFac = GivenCaptureDeviceFactory();

            return new ProcessWatchDog(cmdHelper, xmlHelper, captureWriterFactory: fileWriterFac, captureDeviceFactory: captureDeviceFac);
        }

        private PacketCaptureProcessInfo GivenValidPacketCaptureInfo()
        {            
            Mock<PacketCaptureProcessInfo> packetInfo = new Mock<PacketCaptureProcessInfo>();
            Process process = WatchDogTestHelper.GivenFirstRunningProcess();

            packetInfo.SetupGet(p => p.Process).Returns(process);
            packetInfo.SetupGet(p => p.ProcessName).Returns(process.ProcessName);
            packetInfo.SetupGet(p => p.PID).Returns(process.Id);
            packetInfo.SetupGet(p => p.Ports).Returns(new int[] { 8080});

            return packetInfo.Object;
        }

        private PacketCaptureProcessInfo GivenInvalidPacketCaptureInfo()
        {
            Mock<PacketCaptureProcessInfo> packetInfo = new Mock<PacketCaptureProcessInfo>();
            Process process = WatchDogTestHelper.GivenFakeProcess();

            packetInfo.SetupGet(p => p.Process).Returns(process);
            packetInfo.SetupGet(p => p.ProcessName).Returns("A_Process");
            packetInfo.SetupGet(p => p.PID).Returns(33);
            packetInfo.SetupGet(p => p.Ports).Returns(new int[] { 8080 });

            return packetInfo.Object;
        }

        private XmlHelper GivenXmlHelper()
        {
            Mock<XmlHelper> xmlHelper = new Mock<XmlHelper>();
            xmlHelper.Setup(x => x.DeserializeConfiguration<WatchdogInitialization>(It.IsAny<string>())).Returns(new WatchdogInitialization() { InitialProcess2watchNames = new List<string>() { INITIAL_TARGET } });
            return xmlHelper.Object;
        }

        ICaptureFactory<ICaptureFileWriter> GivenFileWriterFactory()
        {
            Mock<ICaptureFactory<ICaptureFileWriter>> factory = new Mock<ICaptureFactory<ICaptureFileWriter>>();
            Mock<ICaptureFileWriter> fileWriter = new Mock<ICaptureFileWriter>();

            fileWriter.Setup(fw => fw.Write(It.IsAny<CaptureEventWrapperArgs>())).Callback(() => _packetExchanged += "1");
            factory.Setup(f => f.CreateInstance(It.IsAny<string>())).Returns(fileWriter.Object);

            return factory.Object;
        }

        ICaptureFactory<IPacketCaptureDevice> GivenCaptureDeviceFactory()
        {
            Mock<IPacketCaptureDevice> device = GivenCaptureDeviceMock();
            Mock<CaptureEventWrapperArgs> eventArgs = new Mock<CaptureEventWrapperArgs>();
            Mock<PacketWrapper> packets = new Mock<PacketWrapper>();

            eventArgs.SetupGet(ea => ea.Packet).Returns(packets.Object);
            packets.SetupGet(p => p.Data).Returns(_captureData);
            
            device.Setup(d => d.StartCapture())
                  .Raises(d => d.OnPacketArrival += null, device.Object, eventArgs.Object);
            device.SetupRemove(d => d.OnPacketArrival -= It.IsAny<PacketCaptureEventHandlerWrapper>()).Callback(() => _handlerRegistered = false);
            device.SetupAdd(d => d.OnPacketArrival += It.IsAny<PacketCaptureEventHandlerWrapper>()).Callback(() => _handlerRegistered = true);

            Mock<ICaptureFactory<IPacketCaptureDevice>> factory = GivenCaptureDeviceFactoryMock(device.Object);

            return factory.Object;
        }

        private Mock<ICaptureFactory<IPacketCaptureDevice>> GivenCaptureDeviceFactoryMock(IPacketCaptureDevice device)
        {
            Mock<ICaptureFactory<IPacketCaptureDevice>> factory = new Mock<ICaptureFactory<IPacketCaptureDevice>>();
            factory.Setup(f => f.CreateInstance(It.IsAny<string>())).Returns(device);
            return factory;
        }

        private Mock<IPacketCaptureDevice> GivenCaptureDeviceMock()
        {
            Mock<IPacketCaptureDevice> device = new Mock<IPacketCaptureDevice>();
            device.Setup(d => d.Open(It.IsAny<int>())).Callback(() => _deviceIsOpen = true);
            return device;
        }

        #endregion

    }
}
