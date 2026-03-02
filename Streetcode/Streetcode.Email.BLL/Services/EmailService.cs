using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;
using Microsoft.Extensions.Options;
using Streetcode.Email.BLL.Configs;
using Streetcode.Email.BLL.DTO;
using Streetcode.Email.BLL.Interfaces;

namespace Streetcode.Email.BLL.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailConfiguration _emailConfig;

        public EmailService(IOptions<EmailConfiguration> options)
        {
            _emailConfig = options.Value;
        }

        public async Task SendEmailAsync(EmailDTO email)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Streetcode", _emailConfig.FromAddress));
            message.To.Add(new MailboxAddress("Streetcode Admin", _emailConfig.AdminAddress));
            message.Subject = $"New feedback from Streetcode User";
            message.Body = new TextPart("plain")
            {
                Text = $"Користувач {email.From} залишив повідомлення:\n\n{email.Content}"
            };

            using var client = new SmtpClient();

            await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, SecureSocketOptions.Auto);
            await client.AuthenticateAsync(_emailConfig.SmtpUser, _emailConfig.SmtpPassword);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
