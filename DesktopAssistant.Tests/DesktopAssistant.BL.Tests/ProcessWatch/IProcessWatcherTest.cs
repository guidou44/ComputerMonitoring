using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using Common.Helpers;
using DesktopAssistant.BL;
using DesktopAssistant.BL.ProcessWatch;
using DesktopAssistant.Tests.DesktopAssistant.BL.Tests.ProcessWatch.Exceptions;
using DesktopAssistant.Tests.DesktopAssistant.Tests.Helper;
using DesktopAssistant.Tests.ProcessMonitoring.Tests.Helpers;
using Moq;
using ProcessMonitoring.Models;
using SharpPcap;
using Xunit;

namespace DesktopAssistant.Tests.DesktopAssistant.BL.Tests.ProcessWatch
{
    public abstract class IProcessWatcherTest
    {
        private const string InitialTarget = "chrome";
        
        private bool _captureDeviceOpen = false;
        private bool _jobTimerRunning = false;
        private ElapsedEventHandler _watchJobHandler;
        private string _captureDeviceFilter = string.Empty;
        private readonly List<int> _openPorts = new List<int>(){1, 2, 3, 4};

        [Fact]
        public void GivenConfigFile_WhenGettingInitialProcessToWatch_ThenItReturnsProperProcessWatch()
        {
            IProcessWatcher watchdog = GivenProcessWatcher();

            IEnumerable<IProcessWatch> initialTargets = watchdog.GetProcessUnderWatch();

            var processWatches = initialTargets as IProcessWatch[] ?? initialTargets.ToArray();
            Assert.Single(processWatches);
            Assert.Contains(InitialTarget, processWatches.Single().ProcessName);
        }
        
        [Fact]
        public void GivenRunningProcess_WhenAddProcessWatch_ProcessIsRunning()
        {
            IProcessWatcher watchdog = GivenProcessWatcher();
            Process runningProcess = ProcessWatchTestHelper.GivenFirstRunningProcess();

            watchdog.AddProcessToWatchList(runningProcess.ProcessName, false);
            IEnumerable<IProcessWatch> processWatches = watchdog.GetProcessUnderWatch();
            IProcessWatch processWatch =
                processWatches.SingleOrDefault(pw => pw.ProcessName.Equals(runningProcess.ProcessName));

            Assert.True(processWatch?.IsRunning) ;
            Assert.Equal(2, processWatches.Count());
        }

        [Fact]
        public void GivenNotRunningProcess_WhenAddProcessWatch_ProcessIsNotRunning()
        {
            const string processName = "NOT_A_PROCESS";
            IProcessWatcher watchdog = GivenProcessWatcher();

            watchdog.AddProcessToWatchList(processName, false);
            IEnumerable<IProcessWatch> processWatches = watchdog.GetProcessUnderWatch();
            IProcessWatch processWatch =
                processWatches.SingleOrDefault(pw => pw.ProcessName.Equals(processName));

            Assert.False(processWatch?.IsRunning) ;
        }

        [Fact]
        public void GivenWatchJob_WhenStartWatch_ThenItRegistersHandler()
        {
            IProcessWatcher watchdog = GivenProcessWatcher();
            
            watchdog.StartCapture();

            Assert.True(_jobTimerRunning);
            Assert.NotNull(_watchJobHandler);
        }

        [Fact]
        public void GivenWatchJob_WhenStopWatch_ThenItUnregistersHandler()
        {
            _watchJobHandler = null;
            _jobTimerRunning = false;
            _captureDeviceOpen = true;
            IProcessWatcher watchdog = GivenProcessWatcher();
            
            watchdog.StartCapture();
            watchdog.StopCapture();
            
            Assert.False(_jobTimerRunning);
            Assert.Null(_watchJobHandler);
            Assert.False(_captureDeviceOpen);
        }

        [Fact]
        public void GivenAddedProcessWatchWithNoCapture_WhenStartCapture_CaptureDeviceFilterDoesntChange()
        {
            Process runningProcess = ProcessWatchTestHelper.GivenFirstRunningProcess();
            IProcessWatcher watchdog = GivenProcessWatcher();
            watchdog.AddProcessToWatchList(runningProcess.ProcessName, false);
            watchdog.StartCapture();
            
            _watchJobHandler.Invoke(null, new EventArgs() as ElapsedEventArgs);
            
            Assert.Equal(string.Empty, _captureDeviceFilter);
        }

        [Fact]
        public void GivenUpdateProcessWatchToNoCapture_WhenStartCapture_ThenItRemovesProcessPortsFromWatchList()
        {
            Process runningProcess = ProcessWatchTestHelper.GivenFirstRunningProcess();
            IProcessWatcher watchdog = GivenProcessWatcher();
            watchdog.AddProcessToWatchList(runningProcess.ProcessName, true);
            watchdog.UpdateProcessCaptureInWatchList(runningProcess.ProcessName, false);
            watchdog.StartCapture();
            
            _watchJobHandler.Invoke(null, new EventArgs() as ElapsedEventArgs);
            
            Assert.Equal(string.Empty, _captureDeviceFilter);
        }

        [Fact]
        public void GivenProcessUnderWatch_WhenRemoveProcess_ThenItRemovesProperly()
        {
            Process runningProcess = ProcessWatchTestHelper.GivenFirstRunningProcess();
            IProcessWatcher watchdog = GivenProcessWatcher();
            watchdog.AddProcessToWatchList(runningProcess.ProcessName, false);
            IEnumerable<IProcessWatch> processWatches = watchdog.GetProcessUnderWatch();
            Assert.Equal(2, processWatches.Count());
            
            watchdog.RemoveProcessFromWatchList(runningProcess.ProcessName);
            processWatches = watchdog.GetProcessUnderWatch();
            
            Assert.Single(processWatches);
        }

        #region Private Methods

        private IProcessWatcher GivenProcessWatcher()
        {
            Mock<NetworkHelper> networkHelperMock = GivenNetworkHelperMock();
            Mock<XmlHelper> xmlHelper = GivenXmlHelperMock();
            Mock<ICaptureDeviceFactory> factory = GivenCaptureDeviceFactoryMock();
            Mock<ITimer> timerMock = GivenTimerMock();
            return GivenProcessWatcher(networkHelperMock.Object, xmlHelper.Object, factory.Object, timerMock.Object);
        }

        private Mock<NetworkHelper> GivenNetworkHelperMock()
        {
            CommandLineHelper cmdHelper = new CommandLineHelper();
            Mock<NetworkHelper> netHelperMock = new Mock<NetworkHelper>(cmdHelper);
            netHelperMock.Setup(nh => nh.GetOpenTcpAndUdpPortsForProcess(It.IsAny<Process>(), null))
                .Returns(_openPorts);
            netHelperMock.Setup(nh => nh.GetOpenTcpAndUdpPortsForProcess(It.IsAny<Process>(), It.IsAny<int?>()))
                .Returns(_openPorts);
            return netHelperMock;
        }
        
        private Mock<XmlHelper> GivenXmlHelperMock()
        {
            Mock<XmlHelper> xmlHelper = new Mock<XmlHelper>();
            xmlHelper.Setup(x => x.DeserializeConfiguration<WatchdogInitialization>(It.IsAny<string>())).Returns(new WatchdogInitialization() { InitialProcess2WatchNames = new List<string>() { InitialTarget } });
            return xmlHelper;
        }
        
        private Mock<ICaptureDeviceFactory> GivenCaptureDeviceFactoryMock()
        {
            Mock<ICaptureDevice> device = new Mock<ICaptureDevice>();
            Mock<ICaptureDeviceFactory> factory = new Mock<ICaptureDeviceFactory>();
            device.Setup(d => d.Open()).Callback(() => _captureDeviceOpen = true);
            device.Setup(d => d.Close()).Callback(() => _captureDeviceOpen = false);
            device.SetupSet(d => d.Filter = It.IsAny<string>()).Callback<string>(s => _captureDeviceFilter = s);
            factory.Setup(f => f.CreateInstance(It.IsAny<string>())).Returns(device.Object);
            return factory;
        }

        private Mock<ITimer> GivenTimerMock()
        {
            Mock<ITimer> timerMock = new Mock<ITimer>();
            timerMock.Setup(t => t.Start()).Callback(() => _jobTimerRunning = true);
            timerMock.Setup(t => t.Stop()).Callback(() => _jobTimerRunning = false);
            timerMock.SetupAdd(t => t.Elapsed += It.IsAny<ElapsedEventHandler>())
                .Callback<ElapsedEventHandler>(eh => _watchJobHandler = eh);
            timerMock.SetupRemove(t => t.Elapsed -= It.IsAny<ElapsedEventHandler>())
                .Callback(() => _watchJobHandler = null);
            return timerMock;
        }

        protected abstract IProcessWatcher GivenProcessWatcher(NetworkHelper networkHelper, XmlHelper xmlHelper,
            ICaptureDeviceFactory captureDeviceFactory, ITimer jobTimer);
        
        
        #endregion
    }
}