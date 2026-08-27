using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiWarehouse.Service.Services.Interfaces.Common;
using MultiWarehouse.Service.Services.Interfaces.Identity;
using MultiWarehouse.Shared.DTOs;
using MultiWarehouse.Shared.DTOs.AuditLogDtos;
using MultiWarehouse.Shared.DTOs.UserDtos;
using System.Security.Claims;

namespace MultiWarehouse.API.Controllers
{
    /// <summary>
    /// Sistemdeki kullanıcıları (personel, müdür vb.) yöneten, profil ve güvenlik ayarlarını sağlayan API kontrolcüsü.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuditLogService _auditLogService;

        public UsersController(IUserService userService, IAuditLogService auditLogService)
        {
            _userService = userService;
            _auditLogService = auditLogService;
        }

        #region User Management (CRUD)

        /// <summary>
        /// Yeni bir kullanıcı hesabı oluşturur. Manager sadece Staff ekleyebilir.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "SuperAdmin, WarehouseManager")]
        public async Task<IActionResult> Create(UserCreateDto createDto)
        {
            var user = await _userService.CreateUserAsync(createDto);
            return Ok(CustomResponseDto<UserDto>.SuccessResponse(user));
        }

        /// <summary>
        /// Mevcut bir kullanıcının bilgilerini ve yetkilerini günceller.
        /// </summary>
        [HttpPut]
        [Authorize(Roles = "SuperAdmin, WarehouseManager")]
        public async Task<IActionResult> Update(UserUpdateDto updateDto)
        {
            var user = await _userService.UpdateUserAsync(updateDto);
            return Ok(CustomResponseDto<UserDto>.SuccessResponse(user));
        }

        /// <summary>
        /// Belirtilen kullanıcının şifresini sistem yöneticisi tarafından sıfırlar.
        /// </summary>
        [HttpPatch("{id}/reset-password")]
        [Authorize(Roles = "SuperAdmin, WarehouseManager")]
        public async Task<IActionResult> ResetPassword(Guid id, [FromBody] UserResetPasswordDto dto)
        {
            await _userService.ResetPasswordAsync(id, dto.NewPassword);
            return Ok(CustomResponseDto.SuccessResponse());
        }

        /// <summary>
        /// Belirtilen ID'ye sahip kullanıcının detaylarını (Depo bilgisi dahil) getirir.
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin, WarehouseManager")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            return Ok(CustomResponseDto<UserDto>.SuccessResponse(user));
        }

        /// <summary>
        /// Sistemdeki kullanıcıları (Query parametreleri ile filtrelenmiş olarak) listeler.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "SuperAdmin, WarehouseManager")]
        public async Task<IActionResult> GetAll([FromQuery] UserFilterDto filter)
        {
            var users = await _userService.GetAllUsersAsync(filter);
            return Ok(CustomResponseDto<IEnumerable<UserDto>>.SuccessResponse(users));
        }

        /// <summary>
        /// Belirtilen kullanıcının hesap durumunu Aktif ise Pasif, Pasif ise Aktif yapar.
        /// </summary>
        [HttpPatch("{id}/toggle-status")]
        [Authorize(Roles = "SuperAdmin, WarehouseManager")]
        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            await _userService.ToggleUserStatusAsync(id);
            return Ok(CustomResponseDto.SuccessResponse());
        }

        #endregion

        #region Profile Settings

        /// <summary>
        /// Sisteme giriş yapmış olan mevcut kullanıcının kendi profil bilgilerini getirir.
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(CustomResponseDto<string>.FailResponse("Kullanıcı kimliği doğrulanamadı."));

            var userId = Guid.Parse(userIdClaim.Value);
            var user = await _userService.GetUserByIdAsync(userId);
            return Ok(CustomResponseDto<UserDto>.SuccessResponse(user));
        }

        /// <summary>
        /// Giriş yapmış olan kullanıcının kendi profil bilgilerini güncellemesini sağlar.
        /// </summary>
        [HttpPut("me")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UserProfileUpdateDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(CustomResponseDto<string>.FailResponse("Kullanıcı kimliği doğrulanamadı."));

            var userId = Guid.Parse(userIdClaim.Value);
            var updatedUser = await _userService.UpdateProfileAsync(userId, dto);

            return Ok(CustomResponseDto<UserDto>.SuccessResponse(updatedUser));
        }

        /// <summary>
        /// Giriş yapmış olan kullanıcının kendi şifresini değiştirmesini sağlar.
        /// </summary>
        [HttpPut("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(CustomResponseDto<string>.FailResponse("Kullanıcı kimliği doğrulanamadı."));

            var userId = Guid.Parse(userIdClaim.Value);
            await _userService.ChangePasswordAsync(userId, dto);
            return Ok(CustomResponseDto.SuccessResponse());
        }

        #endregion

        #region Email & Logs

        /// <summary>
        /// Kullanıcının e-posta adresini değiştirmesi için talep oluşturur ve onay e-postası gönderir.
        /// </summary>
        [HttpPost("request-email-change")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> RequestEmailChange([FromBody] RequestEmailChangeDto dto)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            await _userService.RequestEmailChangeAsync(userId, dto.NewEmail);
            return Ok(CustomResponseDto.SuccessResponse());
        }

        /// <summary>
        /// E-posta adresine gönderilen doğrulama token'ı ile e-posta değişikliğini onaylar.
        /// </summary>
        [HttpGet("confirm-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmailChange([FromQuery] string token)
        {
            await _userService.ConfirmEmailChangeAsync(token);
            return Ok(CustomResponseDto.SuccessResponse());
        }

        /// <summary>
        /// Giriş yapmış olan kullanıcının kendi hesabına ait son güvenlik ve denetim loglarını listeler.
        /// </summary>
        [HttpGet("my-security-logs")]
        [Authorize]
        public async Task<IActionResult> GetMySecurityLogs()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(CustomResponseDto<string>.FailResponse("Kullanıcı kimliği doğrulanamadı."));

            var logs = await _auditLogService.GetRecentSecurityLogsByUserIdAsync(userId);
            return Ok(CustomResponseDto<IEnumerable<AuditLogDto>>.SuccessResponse(logs));
        }

        #endregion
    }
}