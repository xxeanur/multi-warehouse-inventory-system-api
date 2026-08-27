using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MultiWarehouse.Entity.Entities.Identity;
using MultiWarehouse.Entity.Enums.Common;
using MultiWarehouse.Service.Exceptions;
using MultiWarehouse.Service.Repositories.Interfaces;
using MultiWarehouse.Service.Services.Interfaces.Common;
using MultiWarehouse.Service.Services.Interfaces.Identity;
using MultiWarehouse.Shared.DTOs.AuditLogDtos;
using MultiWarehouse.Shared.DTOs.AuthDtos;
using MultiWarehouse.Shared.DTOs.UserDtos;
using UAParser;

namespace MultiWarehouse.Service.Services.Implementations.Identity
{
    public class AuthService : IAuthService
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _auditLogService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AuthService(
            IGenericRepository<User> userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IPasswordResetTokenRepository passwordResetTokenRepository,
            ITokenService tokenService,
            IUnitOfWork unitOfWork,
            IAuditLogService auditLogService,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _passwordResetTokenRepository = passwordResetTokenRepository;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
            _emailService = emailService;
            _configuration = configuration;
        }

        #region Authentication

        public async Task<TokenDto> LoginAsync(LoginDto loginDto, string userAgent, string ipAddress)
        {
            var user = await _userRepository.Where(x => x.Email == loginDto.Email).SingleOrDefaultAsync();

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                throw new ClientSideException("E-posta veya şifre hatalı.");

            if (!user.IsActive)
                throw new ClientSideException("Hesabınız askıya alınmıştır. Lütfen yönetici ile iletişime geçin.");

            var tokenDto = _tokenService.CreateToken(user);
            var clientInfo = ParseUserAgent(userAgent);

            var newRefreshToken = new RefreshToken
            {
                Token = tokenDto.RefreshToken,
                Expires = tokenDto.RefreshTokenExpiration,
                UserId = user.Id,
                IpAddress = ipAddress,
                DeviceName = clientInfo.Device,
                Browser = clientInfo.Browser,
                LastAccessed = DateTime.UtcNow
            };

            await _refreshTokenRepository.AddAsync(newRefreshToken);

            user.LastLoginDate = DateTime.UtcNow;
            _userRepository.Update(user);

            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.CreateAsync(new AuditLogCreateDto
            {
                UserId = user.Id,
                ActionType = AuditActionType.Login,
                TableName = "Users",
                IpAddress = ipAddress,
                NewValues = $"Cihaz: {clientInfo.Device}, Tarayıcı: {clientInfo.Browser}"
            });

            return tokenDto;
        }

        public async Task<TokenDto> CreateTokenByRefreshTokenAsync(string refreshToken)
        {
            var existRefreshToken = await _refreshTokenRepository.GetByTokenWithUserAsync(refreshToken);

            if (existRefreshToken == null)
                throw new ClientSideException("Refresh token bulunamadı.");

            if (existRefreshToken.Expires < DateTime.UtcNow || existRefreshToken.IsRevoked)
                throw new ClientSideException("Refresh token süresi dolmuş veya iptal edilmiş. Lütfen tekrar giriş yapın.");

            if (!existRefreshToken.User.IsActive)
                throw new ClientSideException("Hesabınız askıya alınmıştır. Oturum yenilenemedi.");

            var tokenDto = _tokenService.CreateToken(existRefreshToken.User);

            existRefreshToken.Token = tokenDto.RefreshToken;
            existRefreshToken.Expires = tokenDto.RefreshTokenExpiration;
            existRefreshToken.LastAccessed = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            return tokenDto;
        }

        public async Task LogoutAsync(string refreshToken)
        {
            var existRefreshToken = await _refreshTokenRepository.Where(x => x.Token == refreshToken).SingleOrDefaultAsync();

            if (existRefreshToken != null)
            {
                existRefreshToken.IsRevoked = true;
                existRefreshToken.RevokedDate = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                await _auditLogService.CreateAsync(new AuditLogCreateDto
                {
                    UserId = existRefreshToken.UserId,
                    ActionType = AuditActionType.Logout,
                    TableName = "Users"
                });
            }
        }

        #endregion

        #region Session Management

        public async Task<IEnumerable<ActiveSessionDto>> GetActiveSessionsAsync(Guid userId, string currentRefreshToken)
        {
            var sessions = await _refreshTokenRepository
                .Where(x => x.UserId == userId && !x.IsRevoked && x.Expires > DateTime.UtcNow)
                .OrderByDescending(x => x.LastAccessed)
                .ToListAsync();

            return sessions.Select(x => new ActiveSessionDto
            {
                Id = x.Id,
                DeviceName = x.DeviceName,
                Browser = x.Browser,
                IpAddress = x.IpAddress,
                CreatedDate = x.CreatedDate,
                LastAccessed = x.LastAccessed,
                IsCurrentSession = x.Token == currentRefreshToken
            });
        }

        public async Task RevokeSessionAsync(Guid userId, Guid tokenId)
        {
            var session = await _refreshTokenRepository.Where(x => x.Id == tokenId && x.UserId == userId).SingleOrDefaultAsync();

            if (session != null)
            {
                session.IsRevoked = true;
                session.RevokedDate = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                await _auditLogService.CreateAsync(new AuditLogCreateDto
                {
                    UserId = userId,
                    ActionType = AuditActionType.SessionRevoked,
                    TableName = "RefreshTokens",
                    NewValues = $"Kapatılan Cihaz: {session.DeviceName} - {session.Browser}"
                });
            }
        }

        public async Task RevokeAllOtherSessionsAsync(Guid userId, string currentRefreshToken)
        {
            var otherSessions = await _refreshTokenRepository
                .Where(x => x.UserId == userId && x.Token != currentRefreshToken && !x.IsRevoked)
                .ToListAsync();

            foreach (var session in otherSessions)
            {
                session.IsRevoked = true;
                session.RevokedDate = DateTime.UtcNow;
            }

            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.CreateAsync(new AuditLogCreateDto
            {
                UserId = userId,
                ActionType = AuditActionType.AllOtherSessionsRevoked,
                TableName = "RefreshTokens",
                NewValues = $"{otherSessions.Count} adet farklı oturum sonlandırıldı."
            });
        }

        #endregion

        #region Password Recovery

        public async Task RequestPasswordResetAsync(string email)
        {
            var user = await _userRepository.Where(x => x.Email == email && x.IsActive).SingleOrDefaultAsync();
            if (user == null) return;

            var resetToken = new PasswordResetToken
            {
                UserId = user.Id,
                Token = Guid.NewGuid().ToString(),
                ExpireDate = DateTime.UtcNow.AddHours(2),
                IsUsed = false
            };

            await _passwordResetTokenRepository.AddAsync(resetToken);
            await _unitOfWork.SaveChangesAsync();


            var clientBaseUrl = _configuration["ClientSettings:BaseUrl"];
            var resetLink = $"{clientBaseUrl}/reset-password?token={resetToken.Token}";

            var mailBody = $"Şifrenizi sıfırlamak için lütfen aşağıdaki linke tıklayın. Bu link 2 saat geçerlidir:\n{resetLink}\n\nEğer böyle bir talepte bulunmadıysanız bu maili dikkate almayın.";

            await _emailService.SendEmailAsync(user.Email, "Şifre Sıfırlama Talebi", mailBody);

            await _auditLogService.CreateAsync(new AuditLogCreateDto
            {
                UserId = user.Id,
                ActionType = AuditActionType.PasswordChanged,
                TableName = "PasswordResetTokens",
                NewValues = "Şifre sıfırlama e-postası gönderildi."
            });
        }

        public async Task ConfirmPasswordResetAsync(ResetPasswordConfirmDto dto)
        {
            var resetToken = await _passwordResetTokenRepository.GetByTokenWithUserAsync(dto.Token);

            if (resetToken == null || resetToken.IsUsed || resetToken.ExpireDate < DateTime.UtcNow)
                throw new ClientSideException("Geçersiz veya süresi dolmuş sıfırlama linki.");

            var user = resetToken.User;
            if (!user.IsActive) throw new ClientSideException("Bu işlem yapılamaz.");

            if (BCrypt.Net.BCrypt.Verify(dto.NewPassword, user.PasswordHash))
                throw new ClientSideException("Yeni şifreniz mevcut şifrenizle aynı olamaz.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            _userRepository.Update(user);

            resetToken.IsUsed = true;
            _passwordResetTokenRepository.Update(resetToken);

            var activeSessions = await _refreshTokenRepository.Where(x => x.UserId == user.Id && !x.IsRevoked).ToListAsync();
            foreach (var session in activeSessions)
            {
                session.IsRevoked = true;
                session.RevokedDate = DateTime.UtcNow;
            }

            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.CreateAsync(new AuditLogCreateDto
            {
                UserId = user.Id,
                ActionType = AuditActionType.PasswordChanged,
                TableName = "Users",
                NewValues = "Şifre 'Şifremi Unuttum' ekranından sıfırlandı ve tüm oturumlar kapatıldı."
            });
        }

        #endregion

        #region Helpers

        private (string Device, string Browser) ParseUserAgent(string userAgent)
        {
            var uaParser = Parser.GetDefault();
            var clientInfo = uaParser.Parse(userAgent);

            var device = clientInfo.Device.Family == "Other" ? clientInfo.OS.Family : clientInfo.Device.Family;
            var browser = clientInfo.UA.Family;

            return (device, browser);
        }

        #endregion
    }
}