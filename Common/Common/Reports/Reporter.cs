using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Xml.Linq;

namespace Common.Reports
{
    public static class Reporter
    {
        public static void LogException(Exception e, string logPath = null, bool addEmailreport = false)
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

            using (var writer = new StreamWriter(logPath ?? defaultLogPath + @"\GeneralExceptions.txt", append: true))
            {
                writer.WriteLine(exEntry);
            }

            if (addEmailreport) SendEmailReport("Exception thrown", exEntry);
        }

        public static void SendEmailReport(string subject, string message)
        {
            MailMessage email = new MailMessage();
            SmtpClient smtpServer = InitializeSmtpClient();
            var recipients = LoadEmailRecipents();
            var sender = ((smtpServer.Credentials as System.Net.NetworkCredential) ??
                throw new Exception("Failed to initialize smtpClient")).UserName;
            email.From = new MailAddress(sender);
            recipients.ToList().ForEach(R => email.To.Add(R));

            email.Subject = subject;
            email.Body = message;

            smtpServer.Send(email);
        }

        private static SmtpClient InitializeSmtpClient()
        {
            SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");
            smtpServer.Port = 587;
            smtpServer.EnableSsl = true;
            var currentDirectory = Directory.GetCurrentDirectory();
            var configuration = XDocument.Load(currentDirectory + @"\ReporterConfiguration.xml");
            var sender = (configuration.Descendants("Sender") ?? throw new ArgumentNullException("No sender specified for email reporter"));

            smtpServer.Credentials = new System.Net.NetworkCredential(sender.Elements("Id").SingleOrDefault().Value, sender.Elements("Password").SingleOrDefault().Value);
            return smtpServer;
        }

        private static IEnumerable<string> LoadEmailRecipents()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var configuration = XDocument.Load(currentDirectory + @"\ReporterConfiguration.xml");

            var recipients = (configuration.Descendants("Receivers") ?? throw new ArgumentNullException("No recipient specified for email reporter"))
                              .Elements("Target");

            return recipients.Select(XE => XE.Elements("Address").SingleOrDefault().Value);
        }
    }
}
