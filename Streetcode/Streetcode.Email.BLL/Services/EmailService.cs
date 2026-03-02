using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Streetcode.Email.BLL.Configs;
using Streetcode.Email.BLL.DTO;
using Streetcode.Email.BLL.Interfaces;
using System.Data;

namespace Streetcode.Email.BLL.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailConfiguration _emailConfig;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailConfiguration> options, ILogger<EmailService> logger)
        {
            _emailConfig = options.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(EmailDTO email)
        {
            try
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

                _logger.LogInformation("Email sent successfully to {Recipient} from {Sender}.", _emailConfig.AdminAddress, email.From);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Recipient} due to an error.", _emailConfig.AdminAddress);

                throw;
            }
        }
    }
}
