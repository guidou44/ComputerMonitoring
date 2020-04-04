﻿using Common.Exceptions;
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
using Xunit;

namespace ComputerMonitoringTests.CommonTests.Reports
{
    public class ReporterTest
    {
        const string ALTERNATE_CONFIG_PATH = @"..\..\Configuration\ReporterConfiguration.xml";

        [Fact]
        public void GivenCustomFilePath_WhenReportingException_ThenFileIsCreated()
        {
            const string cutomFilePath = @".\exceptionLogTest.txt";
            Reporter reporterSubject = new Reporter(ProvideSmtpClient().Object);

            reporterSubject.LogException(new Exception("ex"), cutomFilePath);

            Assert.True(File.Exists(cutomFilePath));
        }

        [Fact]
        public void GivenInvalidFilePath_WhenTryReport_ThenItThrowsProper()
        {
            const string invalidFilePath = "......";
            Reporter reporterSubject = new Reporter(ProvideSmtpClient().Object);

            Assert.Throws<ReporterIOException>(() => reporterSubject.LogException(new Exception("ex"), invalidFilePath));
        }

        [Fact]
        public void GivenNonExistingDirectoryPath_WhenLogException_ThenItCreatesDirectory()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string directoryPath = Path.Combine(currentDirectory, "Exception_Logs");
            GivenDeletedDirectoryIfAlreadyExists(directoryPath);
            Reporter reporterSubject = new Reporter(ProvideSmtpClient().Object);

            reporterSubject.LogException(new Exception("ex"));

            Assert.True(Directory.Exists(directoryPath));
        }

        [Fact]
        public void GivenEmailSwitchTrue_WhenLogException_ThenItSendsEmail()
        {
            Mock<IMailClient> emailClient = ProvideSmtpClient();
            Reporter reporterSubject = new Reporter(emailClient.Object);

            reporterSubject.LogException(new Exception("ex"), addEmailreport: true, emailAlternateConfigPath: ALTERNATE_CONFIG_PATH);

            emailClient.Verify(e => e.Send(It.IsAny<MailMessage>()), Times.Once);
        }

        [Fact]
        public void GivenTargetRecipient_WhenSendingMessage_ThenItSendsToProperRecipient()
        {
            Mock<IMailClient> emailClient = ProvideSmtpClient();
            Reporter reporterSubject = new Reporter(emailClient.Object);

            reporterSubject.SendEmailReport("TEST SUBJECT", "SOME MESSAGE", ALTERNATE_CONFIG_PATH);

            emailClient.Verify(e => e.Send(It.Is<MailMessage>(m => m.To.Any(t => t.Address.Equals("TEST@hotmail.com")))), Times.Once);
        }

        private Mock<IMailClient> ProvideSmtpClient()
        {
            Mock<IMailClient> smtpClient = new Mock<IMailClient>();
            smtpClient.Setup(s => s.Send(It.IsAny<MailMessage>())).Verifiable();
            smtpClient.SetupGet(e => e.Credentials).Returns(new System.Net.NetworkCredential("TEST@gmail.com", "Password"));
            return smtpClient;
        }

        private void GivenDeletedDirectoryIfAlreadyExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath)) 
                Directory.Delete(directoryPath);
        }
    }
}
