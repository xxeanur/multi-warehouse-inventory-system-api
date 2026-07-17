using Microsoft.AspNetCore.Mvc;
using MultiWarehouse.Service.Services.Interfaces;
using MultiWarehouse.Shared.DTOs; // CustomResponseDto için gerekli
using MultiWarehouse.Shared.DTOs.AuthDtos;

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


        /// <summary>
        /// Kullanıcının e-posta ve şifresi ile sisteme giriş yapmasını sağlar.
        /// Access Token ve arka planda yenileme yapacak bir Refresh Token döner.
        /// </summary>
        /// <param name="loginDto">Kullanıcı giriş bilgileri (E-posta ve Şifre)</param>
        /// <returns>İçerisinde JWT ve geçerlilik süreleri olan standart bir yanıt döner.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            // 1. Servisten sadece saf veriyi (TokenDto) al.
            var tokenDto = await _authService.LoginAsync(loginDto);

            // 2. Senin dediğin gibi tam burada, API'nin çıkış noktasında pakete sar ve gönder!
            return Ok(CustomResponseDto<TokenDto>.SuccessResponse(tokenDto));
        }

        /// <summary>
        /// Süresi dolan Access Token'ı, geçerli bir Refresh Token kullanarak yeniler.
        /// </summary>
        /// <param name="refreshTokenDto">Kullanıcının elindeki mevcut Refresh Token verisi</param>
        /// <returns>Yepyeni bir Access Token ve Refresh Token paketi döner.</returns>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> CreateTokenByRefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            // 1. DTO içindeki token metnini alıp servise gönderiyoruz.
            var tokenDto = await _authService.CreateTokenByRefreshTokenAsync(refreshTokenDto.Token);

            // 2. Başarılıysa, 200 OK statü koduyla yeni token paketimizi standart response modelimize sarıp dönüyoruz.
            return Ok(CustomResponseDto<TokenDto>.SuccessResponse(tokenDto));
        }
    }
}