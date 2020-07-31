using Common.Helpers;
using DesktopAssistant.BL;
using DesktopAssistant.BL.ProcessWatch;
using DesktopAssistant.Tests.DesktopAssistant.BL.Tests.ProcessWatch;
using ProcessMonitoring;
using ProcessMonitoring.Models;

namespace DesktopAssistant.Tests.ProcessMonitoring.Tests
{
    public class ProcessWatcherTest : IProcessWatcherTest
    {
        protected override IProcessWatcher GivenProcessWatcher(NetworkHelper networkHelper, XmlHelper xmlHelper,
            ICaptureDeviceFactory captureDeviceFactory, ITimer jobTimer)
        {
            return new ProcessWatcher(networkHelper, xmlHelper, captureDeviceFactory, jobTimer);
        }
    }
}