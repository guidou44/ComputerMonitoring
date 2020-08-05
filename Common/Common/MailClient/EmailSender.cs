using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Xml.Linq;
using Common.Wrappers;

namespace Common.MailClient
{
    public class EmailSender
    {
        private readonly IMailClient _smtpClient;
        private readonly IEnumerable<string> _recipients;

        public EmailSender()
        {
            string server = ConfigurationManager.AppSettings["smtpServer"];
            string configPath = ConfigurationManager.AppSettings["emailConfiguration"];
            _smtpClient = new SmptClientWrapper(server);
            InitializeSmtpClient(configPath);
            _recipients = LoadEmailRecipients(configPath);
        }

        public EmailSender(IMailClient smtpClient)
        {
            string configPath = ConfigurationManager.AppSettings["emailConfiguration"];
            _smtpClient = smtpClient;
            InitializeSmtpClient(configPath);
            _recipients = LoadEmailRecipients(configPath);
        }

        public void SendEmailReport(string subject, string message)
        {
            MailMessage email = new MailMessage();
            var sender = (_smtpClient.Credentials as System.Net.NetworkCredential)?.UserName;
            email.From = new MailAddress(sender);
            _recipients.ToList().ForEach(r => email.To.Add(r));
            email.Subject = subject;
            email.Body = message;
            _smtpClient.Send(email);
        }

        private void InitializeSmtpClient(string configPath)
        {
            _smtpClient.Port = 587;
            _smtpClient.EnableSsl = true;
            var configuration = XDocument.Load(configPath);
            var sender = configuration.Descendants("Sender");
            _smtpClient.Credentials = new System.Net.NetworkCredential(sender.Elements("Id").SingleOrDefault().Value, sender.Elements("Password").SingleOrDefault().Value);
        }

        private static IEnumerable<string> LoadEmailRecipients(string configPath)
        {
            var configuration = XDocument.Load(configPath);
            var recipients = (configuration.Descendants("Receivers").Elements("Target"));
            return recipients.Select(xe => xe.Elements("Address").SingleOrDefault()?.Value);
        }
    }
}