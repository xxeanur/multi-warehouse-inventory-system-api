using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiWarehouse.Service.Services.Interfaces.Identity;
using MultiWarehouse.Shared.DTOs;
using MultiWarehouse.Shared.DTOs.AuthDtos;
using MultiWarehouse.Shared.DTOs.UserDtos;
using System.Security.Claims;

namespace MultiWarehouse.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        #region Login & Token

        /// <summary>
        /// Kullanıcı girişi yapar ve JWT token döner.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var userAgent = Request.Headers["User-Agent"].ToString();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Bilinmiyor";

            var tokenDto = await _authService.LoginAsync(loginDto, userAgent, ipAddress);
            return Ok(CustomResponseDto<TokenDto>.SuccessResponse(tokenDto));
        }

        /// <summary>
        /// Süresi dolan Access Token'ı yeniler.
        /// </summary>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateTokenByRefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            var tokenDto = await _authService.CreateTokenByRefreshTokenAsync(refreshTokenDto.Token);
            return Ok(CustomResponseDto<TokenDto>.SuccessResponse(tokenDto));
        }

        /// <summary>
        /// Mevcut oturumu kapatır ve Refresh Token'ı siler.
        /// </summary>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenDto refreshTokenDto)
        {
            await _authService.LogoutAsync(refreshTokenDto.Token);
            return Ok(CustomResponseDto.SuccessResponse());
        }

        #endregion

        #region Session Management

        /// <summary>
        /// Kullanıcının aktif olan tüm oturumlarını listeler.
        /// </summary>
        [HttpGet("sessions")]
        [Authorize]
        public async Task<IActionResult> GetSessions([FromHeader(Name = "X-Refresh-Token")] string currentRefreshToken)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var sessions = await _authService.GetActiveSessionsAsync(userId, currentRefreshToken);
            return Ok(CustomResponseDto<IEnumerable<ActiveSessionDto>>.SuccessResponse(sessions));
        }

        /// <summary>
        /// Belirtilen oturumu zorla sonlandırır.
        /// </summary>
        [HttpDelete("sessions/{tokenId}")]
        [Authorize]
        public async Task<IActionResult> RevokeSession(Guid tokenId)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            await _authService.RevokeSessionAsync(userId, tokenId);
            return Ok(CustomResponseDto.SuccessResponse());
        }

        /// <summary>
        /// Mevcut cihaz hariç diğer tüm açık oturumları sonlandırır.
        /// </summary>
        [HttpDelete("sessions/revoke-others")]
        [Authorize]
        public async Task<IActionResult> RevokeOtherSessions([FromHeader(Name = "X-Refresh-Token")] string currentRefreshToken)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            await _authService.RevokeAllOtherSessionsAsync(userId, currentRefreshToken);
            return Ok(CustomResponseDto.SuccessResponse());
        }

        #endregion

        #region Password Reset

        /// <summary>
        /// Şifresini unutan kullanıcılar için sıfırlama maili gönderir.
        /// </summary>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            await _authService.RequestPasswordResetAsync(dto.Email);
            return Ok(CustomResponseDto.SuccessResponse());
        }

        /// <summary>
        /// Mailden gelen token ile kullanıcının şifresini kalıcı olarak sıfırlar.
        /// </summary>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordConfirmDto dto)
        {
            await _authService.ConfirmPasswordResetAsync(dto);
            return Ok(CustomResponseDto.SuccessResponse());
        }

        #endregion
    }
}