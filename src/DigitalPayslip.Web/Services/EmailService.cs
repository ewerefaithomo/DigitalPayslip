
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace DigitalPayslip.Web.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        public EmailService(IConfiguration config) => _config = config;

        public async Task SendEmailAsync(string to, string subject, string body, string? attachmentPath = null)
        {
            var smtp = _config.GetSection("Smtp");
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(smtp["FromName"], smtp["FromEmail"]));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            var builder = new BodyBuilder { TextBody = body };
            if (attachmentPath != null && File.Exists(attachmentPath))
                builder.Attachments.Add(attachmentPath);
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(smtp["Host"], smtp.GetValue<int>("Port"), SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(smtp["User"], smtp["Pass"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
