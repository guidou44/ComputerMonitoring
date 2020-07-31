using System;
using System.Diagnostics;
using System.Linq;
using Common.Helpers;
using DesktopAssistant.BL.ProcessWatch;
using DesktopAssistant.Tests.DesktopAssistant.BL.Tests.ProcessWatch;
using DesktopAssistant.Tests.ProcessMonitoring.Tests.Helpers;
using ProcessMonitoring.Models;
using Xunit;

namespace DesktopAssistant.Tests.ProcessMonitoring.Tests.Models
{
    public class ProcessWatchTest : IProcessWatchTest
    {
        [Fact]
        public void GivenNotRunningAndRegisteredReporter_WhenStartRunning_ThenReporterReports()
        {
            ProcessWatch processWatch = new ProcessWatch("test", false, null);
            Assert.False(processWatch.IsRunning);
            
            IProcessReporter reporter = new MockReporter();
            processWatch.RegisterReporter(reporter);
            processWatch.IsRunning = true;

            Assert.Equal(1, ((MockReporter)reporter).ReportedCount);
        }
        
        private class MockReporter : IProcessReporter
        {        
            public int ReportedCount = 0;
            public void ReportProcess(string processName)
            {
                ReportedCount++;
            }
        }

        protected override IProcessWatch GivenProcessWatch(string name, bool doCapture, Process process)
        {
            return new ProcessWatch(name, doCapture, process);
        }
    }
}