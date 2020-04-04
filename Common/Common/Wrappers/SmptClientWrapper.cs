using Common.MailClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Common.Wrappers
{
    public class SmptClientWrapper : SmtpClient, IMailClient 
    {
        public SmptClientWrapper(string server) : base(server) { }        
    }
}
