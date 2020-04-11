using Common.Exceptions;
using Common.MailClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Xml.Linq;

namespace Common.Reports
{
    public class Reporter
    {
        private const string CONFIG_FILE_PATH = @".\Configuration\ReporterConfiguration.xml";
        private IMailClient _smtpClient;

        public Reporter(IMailClient smtpClient)
        {
            _smtpClient = smtpClient;
        }

        public Reporter() { }

        public void LogException(Exception e, string logPath = null, bool addEmailreport = false, string emailAlternateConfigPath = null)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            if (!Directory.Exists(Path.Combine(currentDirectory, "Exception_Logs"))) Directory.CreateDirectory(Path.Combine(currentDirectory, "Exception_Logs"));
            var defaultLogPath = Path.Combine(currentDirectory, "Exception_Logs");

            string exEntry = "\n*************************Exception******************************\n" +
                             "DateTime: " + DateTime.Now.ToString("dd/MM/yyyy H:mm:ss") + "\n" +
                             "Type: " + e.GetType().ToString() + "\n" +
                             "Source: " + e.Source + "\n" +
                             "TargetSite: " + e.TargetSite + "\n" +
                             "Message: " + e.Message + "\n" +
                             "Stacktrace: " + e.StackTrace + "\n";

            try
            {
                using (var writer = new StreamWriter(logPath ?? defaultLogPath + @"\GeneralExceptions.txt", append: true))
                {
                    writer.WriteLine(exEntry);
                }
            }
            catch (Exception ex)
            {
                throw new ReporterIOException(ex.Message);
            }

            if (addEmailreport) SendEmailReport("Exception thrown", exEntry, emailAlternateConfigPath);
        }

        public void SendEmailReport(string subject, string message, string configPath = null)
        {
            MailMessage email = new MailMessage();
            InitializeSmtpClient(configPath ?? CONFIG_FILE_PATH);
            var recipients = LoadEmailRecipents(configPath ?? CONFIG_FILE_PATH);
            var sender = (_smtpClient.Credentials as System.Net.NetworkCredential).UserName;
            email.From = new MailAddress(sender);
            recipients.ToList().ForEach(R => email.To.Add(R));

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

        private IEnumerable<string> LoadEmailRecipents(string configPath)
        {
            var configuration = XDocument.Load(configPath);

            var recipients = (configuration.Descendants("Receivers").Elements("Target"));

            return recipients.Select(XE => XE.Elements("Address").SingleOrDefault().Value);
        }
    }
}
