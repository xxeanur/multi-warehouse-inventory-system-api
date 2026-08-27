using MultiWarehouse.Entity.Entities.Identity;

namespace MultiWarehouse.Service.Repositories.Interfaces
{
    public interface IPasswordResetTokenRepository : IGenericRepository<PasswordResetToken>
    {
        /// <summary>
        /// Şifre sıfırlama token'ını User nesnesi ile birlikte include ederek getirir.
        /// </summary>
        Task<PasswordResetToken?> GetByTokenWithUserAsync(string token);
    }
}