namespace MultiWarehouse.Service.Services.Interfaces.Identity
{
    public interface IEmailService
    {
        /// <summary>
        /// Belirtilen e-posta adresine SMTP üzerinden mail gönderir.
        /// </summary>
        Task SendEmailAsync(string to, string subject, string body);
    }
}