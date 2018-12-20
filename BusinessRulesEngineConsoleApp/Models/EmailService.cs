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
            
            // todo - if error count is more than 0 then add attachment
            mailMessage.Attachments.Add(attachment);
            smtpClient.Send(mailMessage);

            Log.Info($"EMAILED report successfully sent to {string.Join(",",emailRecipients)}");
        }

        private SmtpClient GetSmtpClient()
        {
            var smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
            var smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
            
            var smtpClient = new SmtpClient(smtpHost, smtpPort)
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

            if (sendTo.Count == 0)
            {
                throw new Exception("No recipients to send to. Table - [rules].[RuleValidationRecipients] is empty, please insert one or more email addresses and try again.");
            }

            // Call IsEmailValid on each address.
            sendTo.ForEach(IsEmailValid);
            sendTo.ForEach(address => mailMessage.To.Add(address));

            return mailMessage;
        }

        private void IsEmailValid(string emailAddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailAddress);
            }
            catch
            {
                throw new Exception($"\"{emailAddress}\" is not a valid email address. Table - [rules].[RuleValidationRecipients].");
            }
        }
    }
}
