using Common.Helpers;
using DesktopAssistantTests.ProcessMonitoringTests.Helpers;
using ProcessMonitoring.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DesktopAssistantTests.ProcessMonitoringTests.Models
{
    public class PacketCaptureProcessInfoTest
    {
        [Fact]
        public void GivenRunningProcess_WhenGettingProcessInfo_ThenItReturnsProper()
        {
            Process process = WatchDogTestHelper.GivenFirstRunningProcess();
            PacketCaptureProcessInfo subject = new PacketCaptureProcessInfo(process);

            Assert.Equal(process.Id, subject.PID);
            Assert.Equal(process.ProcessName, subject.ProcessName);
        }

        [Fact]
        public void GivenCmdOutputAndOpenTCPPorts_WhenSetOpenPorts_ThenItSetsProperPorts()
        {
            Process process = WatchDogTestHelper.GivenFirstRunningProcess();
            PacketCaptureProcessInfo subject = new PacketCaptureProcessInfo(process);
            CommandLineHelper cmdHelper = WatchDogTestHelper.GivenCommandLineHelper();

            subject.SetOpenTCPandUDPportsForProcess(cmdHelper.ExecuteCommand("netstat -ano"), 888);

            Assert.True(subject.Ports.Count() == 2);
            Assert.All(subject.Ports, p => Assert.Equal(135, p));
        }

        [Fact]
        public void GivenCmdOutputAndOpenUDPPorts_WhenSetOpenPorts_ThenItSetsProperPorts()
        {
            Process process = WatchDogTestHelper.GivenFirstRunningProcess();
            PacketCaptureProcessInfo subject = new PacketCaptureProcessInfo(process);
            CommandLineHelper cmdHelper = WatchDogTestHelper.GivenCommandLineHelper();

            subject.SetOpenTCPandUDPportsForProcess(cmdHelper.ExecuteCommand("netstat -ano"), 4640);

            Assert.True(subject.Ports.Count() == 4);
            Assert.True(subject.Ports.Where(p => p == 4500).Count() == 2);
            Assert.True(subject.Ports.Where(p => p == 500).Count() == 2);
        }

        [Fact]
        public void GivenCmdOutputAndNoOpenPorts_WhenSetOpenPorts_ThenItSetsNoPorts()
        {
            Process process = WatchDogTestHelper.GivenFirstRunningProcess();
            PacketCaptureProcessInfo subject = new PacketCaptureProcessInfo(process);
            CommandLineHelper cmdHelper = WatchDogTestHelper.GivenCommandLineHelper();

            subject.SetOpenTCPandUDPportsForProcess(cmdHelper.ExecuteCommand("netstat -ano"), 696969);

            Assert.True(subject.Ports.Count() == 0);
        }

        [Fact]
        public void GivenCmdOutputAndNullProcess_WhenSetOpenPorts_ThenItThrowsProper()
        {
            PacketCaptureProcessInfo subject = new PacketCaptureProcessInfo();
            CommandLineHelper cmdHelper = WatchDogTestHelper.GivenCommandLineHelper();

            Assert.Throws<ArgumentNullException>(() => subject.SetOpenTCPandUDPportsForProcess(cmdHelper.ExecuteCommand("netstat -ano"), 1));
        }
    }
}
