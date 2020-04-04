using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

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
