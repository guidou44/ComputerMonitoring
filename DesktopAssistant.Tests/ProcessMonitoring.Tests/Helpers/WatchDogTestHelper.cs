using Common.Helpers;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopAssistantTests.ProcessMonitoringTests.Helpers
{
    public static class WatchDogTestHelper
    {
        private const string CMD_OUTPUT_TXT_FILE = @"..\..\Configuration\cmdOutput.txt";

        public static CommandLineHelper GivenCommandLineHelper()
        {
            Mock<CommandLineHelper> cmdHelper = new Mock<CommandLineHelper>();
            cmdHelper.Setup(c => c.ExecuteCommand("netstat -ano")).Returns(new StreamReader(CMD_OUTPUT_TXT_FILE));
            return cmdHelper.Object;
        }

        public static Process GivenFirstRunningProcess()
        {
            Process[] allprocesses = Process.GetProcesses();
            return allprocesses.First();
        }

        public static Process GivenSecondRunningProcess()
        {
            Process[] allprocesses = Process.GetProcesses();
            return allprocesses.ElementAt(1);
        }

        public static Process GivenFakeProcess()
        {
            return new Process();
        }
    }
}
