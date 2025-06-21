using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;
using UserService.Dto.Email;

namespace UserService.Services.ServiceSendEmail
{
    public class EmailService:IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var message = new MailMessage();
            message.To.Add(new MailAddress(toEmail));
            message.From = new MailAddress(_settings.SenderEmail, _settings.SenderName);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = false;

            using var client = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort)
            {
                Credentials = new NetworkCredential(_settings.SenderEmail, _settings.SenderPassword),
                EnableSsl = true
            };

            await client.SendMailAsync(message);
        }
    }
}
