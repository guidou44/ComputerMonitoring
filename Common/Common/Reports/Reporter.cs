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
        public void LogException(Exception e, string logDirectory)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            if (!Directory.Exists(Path.Combine(currentDirectory, logDirectory))) Directory.CreateDirectory(Path.Combine(currentDirectory, logDirectory));
            var defaultLogPath = Path.Combine(currentDirectory, logDirectory);

            string exEntry = "\n*************************Exception******************************\n" +
                             "DateTime: " + DateTime.Now.ToString("dd/MM/yyyy H:mm:ss") + "\n" +
                             "Type: " + e.GetType().ToString() + "\n" +
                             "Source: " + e.Source + "\n" +
                             "TargetSite: " + e.TargetSite + "\n" +
                             "Message: " + e.Message + "\n" +
                             "Stacktrace: " + e.StackTrace + "\n";

            try
            {
                using (var writer = new StreamWriter(logDirectory + $@"\{logDirectory}.txt", append: true))
                {
                    writer.WriteLine(exEntry);
                }
            }
            catch (Exception ex)
            {
                throw new ReporterIOException(ex.Message);
            }
        }
    }
}
