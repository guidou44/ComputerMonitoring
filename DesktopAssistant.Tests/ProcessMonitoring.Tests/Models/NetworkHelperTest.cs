using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Common.Helpers;
using DesktopAssistant.Tests.ProcessMonitoring.Tests.Helpers;
using ProcessMonitoring.Models;
using Xunit;

namespace DesktopAssistant.Tests.ProcessMonitoring.Tests.Models
{
    public class NetworkHelperTest
    {
        [Fact]
        public void GivenCmdOutputAndNullProcess_WhenGetOpenPorts_ThenItThrowsProper()
        {
            CommandLineHelper cmdHelper = ProcessWatchTestHelper.GivenCommandLineHelper();
            NetworkHelper networkHelper = new NetworkHelper(cmdHelper);

            Assert.Throws<ArgumentNullException>(() => networkHelper.GetOpenTcpAndUdpPortsForProcess(null));
        }
        
        [Fact]
        public void GivenCmdOutputAndNoOpenPorts_WhenGetOpenPorts_ThenItReturnsNoPorts()
        {
            Process process = ProcessWatchTestHelper.GivenFirstRunningProcess();
            CommandLineHelper cmdHelper = ProcessWatchTestHelper.GivenCommandLineHelper();
            NetworkHelper networkHelper = new NetworkHelper(cmdHelper);

            IEnumerable<int> ports = networkHelper.GetOpenTcpAndUdpPortsForProcess(process);

            Assert.False(ports.Any());
        }
        
        [Fact]
        public void GivenCmdOutputAndOpenUDPPorts_WhenGetOpenPorts_ThenItReturnsProperPorts()
        {
            Process process = ProcessWatchTestHelper.GivenFirstRunningProcess();
            CommandLineHelper cmdHelper = ProcessWatchTestHelper.GivenCommandLineHelper();
            NetworkHelper networkHelper = new NetworkHelper(cmdHelper);

            IEnumerable<int> ports = networkHelper.GetOpenTcpAndUdpPortsForProcess(process, 4640);

            Assert.Equal(4, ports.Count());
            Assert.Equal(2, ports.Count(p => p == 4500));
            Assert.Equal(2, ports.Count(p => p == 500));
        }
        
        [Fact]
        public void GivenCmdOutputAndOpenTCPPorts_WhenSetOpenPorts_ThenItSetsProperPorts()
        {
            Process process = ProcessWatchTestHelper.GivenFirstRunningProcess();
            CommandLineHelper cmdHelper = ProcessWatchTestHelper.GivenCommandLineHelper();
            NetworkHelper networkHelper = new NetworkHelper(cmdHelper);

            IEnumerable<int> ports = networkHelper.GetOpenTcpAndUdpPortsForProcess(process, 888);

            Assert.Equal(2, ports.Count());
            Assert.All(ports, p => Assert.Equal(135, p));
        }
    }
}