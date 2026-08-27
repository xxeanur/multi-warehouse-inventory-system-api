using MultiWarehouse.Shared.DTOs.AuthDtos;
using MultiWarehouse.Shared.DTOs.UserDtos;

namespace MultiWarehouse.Service.Services.Interfaces.Identity
{
    public interface IAuthService
    {
        #region Authentication

        /// <summary>
        /// Kullanıcı girişini doğrular ve JWT token üretir.
        /// </summary>
        Task<TokenDto> LoginAsync(LoginDto loginDto, string userAgent, string ipAddress);

        /// <summary>
        /// Süresi dolan Access Token'ı Refresh Token kullanarak yeniler.
        /// </summary>
        Task<TokenDto> CreateTokenByRefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Kullanıcının mevcut oturumunu kapatır ve token'ı iptal eder.
        /// </summary>
        Task LogoutAsync(string refreshToken);

        #endregion

        #region Session Management

        /// <summary>
        /// Kullanıcının aktif olan tüm oturumlarını getirir.
        /// </summary>
        Task<IEnumerable<ActiveSessionDto>> GetActiveSessionsAsync(Guid userId, string currentRefreshToken);

        /// <summary>
        /// Kullanıcının belirtilen spesifik oturumunu sonlandırır.
        /// </summary>
        Task RevokeSessionAsync(Guid userId, Guid tokenId);

        /// <summary>
        /// Mevcut oturum hariç, kullanıcının diğer tüm açık oturumlarını sonlandırır.
        /// </summary>
        Task RevokeAllOtherSessionsAsync(Guid userId, string currentRefreshToken);

        #endregion

        #region Password Recovery

        /// <summary>
        /// Şifresini unutan kullanıcılar için sıfırlama e-postası gönderir.
        /// </summary>
        Task RequestPasswordResetAsync(string email);

        /// <summary>
        /// E-postadan gelen doğrulama token'ı ile kullanıcının yeni şifresini kaydeder.
        /// </summary>
        Task ConfirmPasswordResetAsync(ResetPasswordConfirmDto dto);

        #endregion
    }
}