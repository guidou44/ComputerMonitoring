using Common.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace ComputerMonitoringTests.CommonTests.Helpers
{
    public class CommandLineHelperTest
    {
        private CommandLineHelper commandLineHelperSubject;

        public CommandLineHelperTest()
        {
            commandLineHelperSubject = new CommandLineHelper();
        }

        [Fact]
        public void GivenValidCmdInput_WhenRuningInCmd_ThenItReturnsNormalCmdOutput()
        {
            StreamReader reader = commandLineHelperSubject.ExecuteCommand("ipconfig");
            string line = null;

            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Contains("IPv4 Address"))
                    break;
            }

            Assert.Contains("IPv4 Address", line);
        }

        [Fact]
        public void GivenInvalidCmdInput_WhenRuningInCmd_ThenItReturnsNormalCmdOutput()
        {
            const string invalidCommand = "not a command";
            StreamReader reader = commandLineHelperSubject.ExecuteCommand(invalidCommand);
            string line = null;
            string lastLine = null;
            int importantLinesCounter = 0;

            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Contains(invalidCommand))
                {
                    importantLinesCounter += 1;
                    lastLine = line;
                }
                else if (importantLinesCounter > 0)
                {
                    lastLine += line;
                    if (importantLinesCounter >= 2) break;
                }
            }

            Assert.Contains(invalidCommand, lastLine);
            Assert.Contains("exit", lastLine);
        }
    }
}
