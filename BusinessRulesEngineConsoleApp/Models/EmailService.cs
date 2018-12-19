using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace BusinessRulesEngineConsoleApp.Models
{
    public interface IEmailService
    {
        void SendReportEmail(List<string> emailRecipients, string csvName, string body);
    }
    public class EmailService : IEmailService
    {
        private readonly string _senderEmail = ConfigurationManager.AppSettings["EmailAddress"];
        private readonly string _senderPassword = ConfigurationManager.AppSettings["EmailPassword"];
        public readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void SendReportEmail(List<string> emailRecipients, string csvName, string body)
        {
            var smtpClient = GetSmtpClient();
            
            var mailMessage = GetMailMessage(_senderEmail, emailRecipients, body);
            
            var csv = ConfigurationManager.AppSettings["ReportDirectory"] + $"\\{csvName}";
            var attachment = new Attachment(csv, new ContentType("text/csv"))
            {
                Name = csvName
            };
            
            mailMessage.Attachments.Add(attachment);
            smtpClient.Send(mailMessage);

            Log.Info($"EMAILED report successfully sent to {string.Join(",",emailRecipients)}");
        }

        private SmtpClient GetSmtpClient()
        {
            var smtpClient = new SmtpClient("smtp.office365.com", 587)
            {
                UseDefaultCredentials = false
            };

            var basicAuthenticationInfo = new
                NetworkCredential(_senderEmail, _senderPassword);
            smtpClient.Credentials = basicAuthenticationInfo;
            smtpClient.EnableSsl = true;

            return smtpClient;
        }

        private MailMessage GetMailMessage(string from, List<string> sendTo, string body)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(from),
                Subject = $"Ods validation report {DateTime.Now:MM-dd-yyyy}",
                SubjectEncoding = Encoding.UTF8,
                Body = body,
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = true
            };

            sendTo.ForEach(address => mailMessage.To.Add(address));

            return mailMessage;
        }
    }
}
