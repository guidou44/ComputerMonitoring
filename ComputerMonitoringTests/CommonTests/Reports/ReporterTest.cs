using Common.Exceptions;
using Common.Reports;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ComputerMonitoringTests.CommonTests.Reports
{
    public class ReporterTest
    {
        [Fact]
        public void GivenCustomFilePath_WhenReportingException_ThenFileIsCreated()
        {
            const string cutomFilePath = @".\exceptionLogTest.txt";

            Reporter.LogException(new Exception("ex"), cutomFilePath);

            Assert.True(File.Exists(cutomFilePath));
        }

        [Fact]
        public void GivenInvalidFilePath_WhenTryReport_ThenItThrowsProper()
        {
            Assert.Throws<ReporterIOException>(() => Reporter.LogException(new Exception("ex"), "......"));
        }
    }
}
