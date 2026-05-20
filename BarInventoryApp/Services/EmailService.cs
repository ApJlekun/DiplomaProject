using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace BarInventoryApp.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailWithAttachmentAsync(string recipientEmail, string subject, string body, string attachmentPath)
        {
            var smtpServer = _configuration["SmtpSettings:Server"];
            var smtpPort = int.Parse(_configuration["SmtpSettings:Port"] ?? "587");
            var senderEmail = _configuration["SmtpSettings:SenderEmail"];
            var senderPassword = _configuration["SmtpSettings:SenderPassword"];

            // Оптимизация сетевых подключений для предотвращения задержек
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
            ServicePointManager.DefaultConnectionLimit = 10;

            using var client = new SmtpClient(smtpServer, smtpPort)
            {
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Timeout = 100000 
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail!),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            mailMessage.To.Add(recipientEmail);

            if (!string.IsNullOrEmpty(attachmentPath))
            {
                // Используем FileStream для гарантии, что файл не занят
                using var fileStream = new System.IO.FileStream(attachmentPath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                mailMessage.Attachments.Add(new Attachment(fileStream, System.IO.Path.GetFileName(attachmentPath)));
                
                await Task.Run(() => client.Send(mailMessage));
            }
            else
            {
                await Task.Run(() => client.Send(mailMessage));
            }
        }
    }
}
