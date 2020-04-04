using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Common.MailClient
{
    public class SmptClientWrapper : SmtpClient, IMailClient 
    {
        public SmptClientWrapper(string server) : base(server) { }        
    }
}
