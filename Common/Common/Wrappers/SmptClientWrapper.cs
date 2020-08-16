using System.Net.Mail;
using Common.MailClient;

namespace Common.Wrappers
{
    public class SmptClientWrapper : SmtpClient, IMailClient 
    {
        public SmptClientWrapper(string server) : base(server) { }
    }
}