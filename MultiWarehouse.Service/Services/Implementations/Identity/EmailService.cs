using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MultiWarehouse.Service.Services.Interfaces.Identity;

namespace MultiWarehouse.Service.Services.Implementations.Identity
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        #region Email Operations

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var emailHost = _configuration["EmailSettings:Host"];
            var emailPort = int.Parse(_configuration["EmailSettings:Port"]!);
            var emailAddress = _configuration["EmailSettings:Email"];
            var emailPassword = _configuration["EmailSettings:Password"];
            var displayName = _configuration["EmailSettings:DisplayName"];

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(displayName, emailAddress));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;

            email.Body = new TextPart(MimeKit.Text.TextFormat.Plain)
            {
                Text = body
            };

            using var smtp = new SmtpClient();
            try
            {
                await smtp.ConnectAsync(emailHost, emailPort, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(emailAddress, emailPassword);

                await smtp.SendAsync(email);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Mail gönderilirken hata oluştu: {ex.Message}");
                throw new Exception("E-posta gönderimi başarısız oldu. Lütfen sistem yöneticisiyle iletişime geçin.");
            }
            finally
            {
                await smtp.DisconnectAsync(true);
            }
        }

        #endregion
    }
}