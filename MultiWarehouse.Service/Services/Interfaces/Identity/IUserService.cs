using MultiWarehouse.Shared.DTOs.UserDtos;

namespace MultiWarehouse.Service.Services.Interfaces.Identity
{
    public interface IUserService
    {
        #region User Management (CRUD)

        /// <summary>
        /// Sisteme yeni bir personel kaydeder.
        /// </summary>
        Task<UserDto> CreateUserAsync(UserCreateDto createDto);

        /// <summary>
        /// Sistemdeki bir personeli günceller.
        /// </summary>
        Task<UserDto> UpdateUserAsync(UserUpdateDto updateDto);

        /// <summary>
        /// Bir personelin şifresini Admin onayıyla sıfırlar.
        /// </summary>
        Task ResetPasswordAsync(Guid id, string newPassword);

        /// <summary>
        /// ID parametresine göre spesifik bir personeli getirir.
        /// </summary>
        Task<UserDto> GetUserByIdAsync(Guid id);

        /// <summary>
        /// Gelen filtrelere ve RBAC yetkilerine göre kullanıcıları listeler.
        /// </summary>
        Task<IEnumerable<UserDto>> GetAllUsersAsync(UserFilterDto filter);

        /// <summary>
        /// Kullanıcının hesap durumunu (Aktif/Pasif) tersine çevirir (Toggle).
        /// </summary>
        Task ToggleUserStatusAsync(Guid id);

        #endregion

        #region Profile & Security

        /// <summary>
        /// Sisteme giriş yapmış kullanıcının kendi kısıtlı bilgilerini güncellemesi için kullanılır.
        /// </summary>
        Task<UserDto> UpdateProfileAsync(Guid userId, UserProfileUpdateDto profileDto);

        /// <summary>
        /// Kullanıcının kendi şifresini değiştirmesini sağlar.
        /// </summary>
        Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto);

        /// <summary>
        /// E-posta değiştirme talebi oluşturur (Token üretir ve mail atar).
        /// </summary>
        Task RequestEmailChangeAsync(Guid userId, string newEmail);

        /// <summary>
        /// Linkteki token ile e-postayı kalıcı olarak günceller.
        /// </summary>
        Task ConfirmEmailChangeAsync(string token);

        #endregion
    }
}