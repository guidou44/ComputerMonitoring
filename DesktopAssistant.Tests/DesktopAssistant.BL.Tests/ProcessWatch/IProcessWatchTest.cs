using System.Diagnostics;
using DesktopAssistant.BL.ProcessWatch;
using DesktopAssistant.Tests.ProcessMonitoring.Tests.Helpers;
using Xunit;

namespace DesktopAssistant.Tests.DesktopAssistant.BL.Tests.ProcessWatch
{
    public abstract class IProcessWatchTest
    {
        protected abstract IProcessWatch GivenProcessWatch(string name, bool doCapture, Process process);
        
        [Fact]
        public void GivenRunningProcess_WhenGettingProcessInfo_ThenItReturnsProper()
        {
            Process process = ProcessWatchTestHelper.GivenFirstRunningProcess();
            
            IProcessWatch subject = GivenProcessWatch(process.ProcessName, false, process);

            Assert.Equal(process.ProcessName, subject.ProcessName);
        }

        [Fact]
        public void GivenRunningProcess_WhenInstantiateProcessWatch_ThenItIsRunning()
        {
            Process process = ProcessWatchTestHelper.GivenFirstRunningProcess();
            
            IProcessWatch subject = GivenProcessWatch(process.ProcessName, false, process);
            
            Assert.True(subject.IsRunning);
        }

        [Fact]
        public void GivenNullProcess_WhenInstantiateProcessWatch_ItIsNotRunning()
        {
            IProcessWatch subject = GivenProcessWatch("TEST", false, null);
            
            Assert.False(subject.IsRunning);
        }
    }
}