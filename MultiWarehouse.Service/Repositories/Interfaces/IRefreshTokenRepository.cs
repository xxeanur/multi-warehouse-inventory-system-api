using MultiWarehouse.Entity.Entities.Identity;

namespace MultiWarehouse.Service.Repositories.Interfaces
{
    public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
    {
        /// <summary>
        /// Refresh token'ı User (Kullanıcı) nesnesi ile birlikte include ederek getirir.
        /// </summary>
        Task<RefreshToken?> GetByTokenWithUserAsync(string token);
    }
}