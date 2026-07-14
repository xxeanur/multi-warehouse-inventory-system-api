using Microsoft.AspNetCore.Mvc;
using MultiWarehouse.Service.Services.Interfaces;
using MultiWarehouse.Shared.DTOs; // CustomResponseDto için gerekli
using MultiWarehouse.Shared.DTOs.AuthDtos;
using System.Threading.Tasks;

namespace multi_warehouse_inventory_system_api.Controllers
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
        /// Kullanıcının e-posta ve şifresi ile sisteme giriş yapmasını (Login) sağlar.
        /// İşlem başarılı olursa kullanıcının kimliğini doğrulayan bir Access Token ve arka planda yenileme yapacak bir Refresh Token döner.
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
    }
}