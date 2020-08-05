using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using Common.Helpers;
using DesktopAssistant.BL;
using DesktopAssistant.BL.Persistence;
using DesktopAssistant.BL.ProcessWatch;
using DesktopAssistant.Tests.DesktopAssistant.BL.Tests.ProcessWatch;
using Moq;
using ProcessMonitoring;
using ProcessMonitoring.Models;
using SharpPcap;

namespace DesktopAssistant.Tests.ProcessMonitoring.Tests
{
    public class ProcessWatcherTest : IProcessWatcherTest
    {
        protected override IProcessWatcher GivenProcessWatcher()
        {
            Mock<NetworkHelper> networkHelperMock = GivenNetworkHelperMock();
            Mock<IRepository> repository = GivenRepositoryMock();
            Mock<ICaptureDeviceFactory> factory = GivenCaptureDeviceFactoryMock();
            Mock<ITimer> timerMock = GivenTimerMock();
            return GivenProcessWatcher(networkHelperMock.Object, repository.Object, factory.Object, timerMock.Object);
        }
        
        private IProcessWatcher GivenProcessWatcher(NetworkHelper networkHelper, IRepository repository,
            ICaptureDeviceFactory captureDeviceFactory, ITimer jobTimer)
        {
            return new ProcessWatcher(networkHelper, repository, captureDeviceFactory, jobTimer);
        }

        private Mock<NetworkHelper> GivenNetworkHelperMock()
        {
            CommandLineHelper cmdHelper = new CommandLineHelper();
            Mock<NetworkHelper> netHelperMock = new Mock<NetworkHelper>(cmdHelper);
            netHelperMock.Setup(nh => nh.GetOpenTcpAndUdpPortsForProcess(It.IsAny<Process>(), null))
                .Returns(OpenPorts);
            netHelperMock.Setup(nh => nh.GetOpenTcpAndUdpPortsForProcess(It.IsAny<Process>(), It.IsAny<int?>()))
                .Returns(OpenPorts);
            return netHelperMock;
        }
        
        private Mock<IRepository> GivenRepositoryMock()
        {
            Mock<IRepository> repositoryMock = new Mock<IRepository>();
            repositoryMock.Setup(x => x.Read<WatchdogInitialization>()).Returns(new WatchdogInitialization() { InitialProcess2WatchNames = new List<string>() { INITIAL_TARGET } });
            return repositoryMock;
        }
        
        private Mock<ICaptureDeviceFactory> GivenCaptureDeviceFactoryMock()
        {
            Mock<ICaptureDevice> device = new Mock<ICaptureDevice>();
            Mock<ICaptureDeviceFactory> factory = new Mock<ICaptureDeviceFactory>();
            device.Setup(d => d.Open()).Callback(() => CaptureDeviceOpen = true);
            device.Setup(d => d.Close()).Callback(() => CaptureDeviceOpen = false);
            device.SetupSet(d => d.Filter = It.IsAny<string>()).Callback<string>(s => CaptureDeviceFilter = s);
            factory.Setup(f => f.CreateInstance(It.IsAny<string>())).Returns(device.Object);
            return factory;
        }

        private Mock<ITimer> GivenTimerMock()
        {
            Mock<ITimer> timerMock = new Mock<ITimer>();
            timerMock.Setup(t => t.Start()).Callback(() => JobTimerRunning = true);
            timerMock.Setup(t => t.Stop()).Callback(() => JobTimerRunning = false);
            timerMock.SetupAdd(t => t.Elapsed += It.IsAny<ElapsedEventHandler>())
                .Callback<ElapsedEventHandler>(eh => WatchJobHandler = eh);
            timerMock.SetupRemove(t => t.Elapsed -= It.IsAny<ElapsedEventHandler>())
                .Callback(() => WatchJobHandler = null);
            return timerMock;
        }
    }
}