using System.Net;
using System.Net.Mail;

namespace Common.MailClient
{
    public interface IMailClient
    {
        int Port { get; set; }
        bool EnableSsl { get; set; }
        ICredentialsByHost Credentials { get; set; }
        void Send(MailMessage message);
    }
}