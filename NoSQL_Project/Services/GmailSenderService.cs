using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using NoSQL_Project.Models.PasswordResset;
using NoSQL_Project.Services.Interfaces;

namespace NoSQL_Project.Services
{
    public class GmailSenderService : IEmailSenderService
    {
        private readonly EmailSettings _emailSettings;
        public GmailSenderService(IOptions<EmailSettings> settings)
        {
            _emailSettings = settings.Value;
        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var mail = new MailMessage()
            {
                From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            mail.To.Add(new MailAddress(email));

            using (var smtp = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort))
            {
                smtp.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(mail);
            }
        }
    }
}
