using Common.Exceptions;
using Common.MailClient;
using Common.Reports;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using DesktopAssistant.Tests.DesktopAssistant.Tests.Helper;
using Xunit;

namespace DesktopAssistant.Tests.Common.Tests.Reports
{
    public class ReporterTest
    {
        
        [Fact]
        public void GivenCustomDirectory_WhenReportingException_ThenFileIsCreatedInProperDirectory()
        {
            const string someDirectory = "SomeDirectory";
            Reporter reporterSubject = new Reporter();

            reporterSubject.LogException(new Exception("ex"), someDirectory);

            Assert.True(File.Exists(someDirectory + $"/{someDirectory}.txt"));
        }

        [Fact]
        public void GivenInvalidFilePath_WhenTryReport_ThenItThrowsProper()
        {
            const string invalidFilePath = "/////";
            Reporter reporterSubject = new Reporter();

            Assert.Throws<ReporterIOException>(() => reporterSubject.LogException(new Exception("ex"), invalidFilePath));
        }

        [Fact]
        public void GivenNonExistingDirectoryPath_WhenLogException_ThenItCreatesDirectory()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string directoryPath = Path.Combine(currentDirectory, "Exception_Logs");
            ComputerMonitoringTestHelper.GivenDeletedDirectoryIfAlreadyExists(directoryPath);
            Reporter reporterSubject = new Reporter();

            reporterSubject.LogException(new Exception("ex"), "Exception_Logs");

            Assert.True(Directory.Exists(directoryPath));
        }
    }
}
