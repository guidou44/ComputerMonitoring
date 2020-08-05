using System.Linq;
using System.Net.Mail;
using Common.MailClient;
using Common.Reports;
using DesktopAssistant.Tests.DesktopAssistant.Tests.Helper;
using Moq;
using Xunit;

namespace DesktopAssistant.Tests.Common.Tests.MailClient
{
    public class EmailSenderTest
    {
        [Fact]
        public void GivenTargetRecipient_WhenSendingMessage_ThenItSendsToProperRecipient()
        {
            Mock<IMailClient> emailClient = ComputerMonitoringTestHelper.ProvideSmtpClient();
            EmailSender senderSubject = new EmailSender(emailClient.Object);

            senderSubject.SendEmailReport("TEST SUBJECT", "SOME MESSAGE");

            emailClient.Verify(e => e.Send(It.Is<MailMessage>(m => m.To.Any(t => t.Address.Equals("TEST@hotmail.com")))), Times.Once);
        }
    }
}