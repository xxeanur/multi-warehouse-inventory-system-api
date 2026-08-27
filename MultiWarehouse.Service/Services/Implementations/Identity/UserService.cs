using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MultiWarehouse.Entity.Entities.Identity;
using MultiWarehouse.Entity.Enums.Common;
using MultiWarehouse.Entity.Enums.User;
using MultiWarehouse.Service.Exceptions;
using MultiWarehouse.Service.Repositories.Interfaces;
using MultiWarehouse.Service.Services.Interfaces.Common;
using MultiWarehouse.Service.Services.Interfaces.Identity;
using MultiWarehouse.Service.Services.Interfaces.Notification;
using MultiWarehouse.Shared.DTOs.AuditLogDtos;
using MultiWarehouse.Shared.DTOs.UserDtos;
using System.Security.Claims;

namespace MultiWarehouse.Service.Services.Implementations.Identity
{
    public class UserService : IUserService
    {
        #region Dependencies

        private readonly IGenericRepository<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly IAuditLogService _auditLogService;
        private readonly INotificationService _notificationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        public UserService(
            IGenericRepository<User> userRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IEmailService emailService,
            IAuditLogService auditLogService,
            INotificationService notificationService,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailService = emailService;
            _auditLogService = auditLogService;
            _notificationService = notificationService;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        #endregion

        #region User Management

        public async Task<UserDto> CreateUserAsync(UserCreateDto createDto)
        {
            createDto.FirstName = createDto.FirstName?.Trim();
            createDto.LastName = createDto.LastName?.Trim();
            createDto.Email = createDto.Email?.Trim().ToLower();

            var isEmailExists = await _userRepository.AnyAsync(x => x.Email == createDto.Email);
            if (isEmailExists) throw new ClientSideException("Bu e-posta adresi zaten sistemde kayıtlı.");

            var currentUserRole = GetCurrentUserRole();
            if (currentUserRole == UserRole.WarehouseManager.ToString())
            {
                var currentWarehouseId = await GetCurrentWarehouseIdAsync();
                if (currentWarehouseId == null)
                    throw new ClientSideException("Sisteme kayıtlı bir deponuz bulunmuyor, kullanıcı ekleyemezsiniz.");

                if (createDto.Role != UserRole.Staff)
                    throw new ClientSideException("Yetki Reddi: Sadece 'Saha Personeli' rolünde kullanıcı ekleyebilirsiniz.");

                if (createDto.WarehouseId != currentWarehouseId)
                    throw new ClientSideException("Yetki Reddi: Sadece kendi deponuza personel ekleyebilirsiniz.");
            }

            var user = _mapper.Map<User>(createDto);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(createDto.Password);
            user.Phone = NormalizePhoneNumber(createDto.Phone);

            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> UpdateUserAsync(UserUpdateDto updateDto)
        {
            updateDto.FirstName = updateDto.FirstName?.Trim();
            updateDto.LastName = updateDto.LastName?.Trim();
            updateDto.Email = updateDto.Email?.Trim().ToLower();

            var user = await _userRepository.Where(x => x.Id == updateDto.Id && x.IsActive).SingleOrDefaultAsync();
            if (user == null) throw new ClientSideException("Güncellenmek istenen aktif kullanıcı bulunamadı.");

            var isEmailExists = await _userRepository.AnyAsync(x => x.Email == updateDto.Email && x.Id != updateDto.Id);
            if (isEmailExists) throw new ClientSideException("Bu e-posta adresi başka bir kullanıcı tarafından kullanılıyor.");

            var currentUserRole = GetCurrentUserRole();
            if (currentUserRole == UserRole.WarehouseManager.ToString())
            {
                var currentWarehouseId = await GetCurrentWarehouseIdAsync();

                if (user.Role == UserRole.SuperAdmin || user.Role == UserRole.WarehouseManager)
                    throw new ClientSideException("Yetki Reddi: Kendi seviyenizdeki veya üstünüzdeki bir hesabı güncelleyemezsiniz.");

                if (user.WarehouseId != currentWarehouseId)
                    throw new ClientSideException("Yetki Reddi: Sadece kendi deponuzdaki personellerin bilgilerini güncelleyebilirsiniz.");

                if (updateDto.Role != user.Role)
                    throw new ClientSideException("Yetki Reddi: Personelin rolünü değiştirme yetkiniz bulunmuyor.");

                if (updateDto.WarehouseId != user.WarehouseId)
                    throw new ClientSideException("Yetki Reddi: Personeli başka bir depoya transfer etme yetkiniz bulunmuyor.");
            }

            user.FirstName = updateDto.FirstName;
            user.LastName = updateDto.LastName;
            user.Email = updateDto.Email;
            user.Role = updateDto.Role;
            user.WarehouseId = updateDto.WarehouseId;
            user.Phone = NormalizePhoneNumber(updateDto.Phone);
            user.AvatarUrl = updateDto.AvatarUrl;

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> GetUserByIdAsync(Guid id)
        {

            var user = await _userRepository.Where(x => x.Id == id).Include(x => x.Warehouse).SingleOrDefaultAsync();
            if (user == null) throw new ClientSideException("Belirtilen ID'ye sahip kullanıcı bulunamadı.");

            var currentUserRole = GetCurrentUserRole();
            var currentUserIdStr = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid.TryParse(currentUserIdStr, out var currentUserId);

            if (user.Id != currentUserId && currentUserRole == UserRole.WarehouseManager.ToString())
            {
                var currentWarehouseId = await GetCurrentWarehouseIdAsync();
                if (user.Role == UserRole.Staff && user.WarehouseId != currentWarehouseId)
                {
                    throw new ClientSideException("Başka bir deponun saha personeline erişim yetkiniz yok.");
                }
            }

            var userDto = _mapper.Map<UserDto>(user);
            userDto.WarehouseName = user.Warehouse?.Name ?? "Depo Atanmamış";
            return userDto;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync(UserFilterDto filter)
        {
            IQueryable<User> query = _userRepository.Where(x => x.IsActive == filter.IsActive).Include(x => x.Warehouse);

            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                var search = filter.SearchText.ToLower().Trim();
                query = query.Where(x => x.FirstName.ToLower().Contains(search) ||
                                         x.LastName.ToLower().Contains(search) ||
                                         x.Email.ToLower().Contains(search));
            }

            if (filter.WarehouseId.HasValue && filter.WarehouseId != Guid.Empty)
            {
                query = query.Where(x => x.WarehouseId == filter.WarehouseId.Value);
            }

            if (filter.Role.HasValue)
            {
                query = query.Where(x => x.Role == filter.Role.Value);
            }

            var currentUserRole = GetCurrentUserRole();
            if (currentUserRole == UserRole.WarehouseManager.ToString())
            {
                var currentWarehouseId = await GetCurrentWarehouseIdAsync();
                query = query.Where(x =>
                    x.Role == UserRole.SuperAdmin ||
                    x.Role == UserRole.WarehouseManager ||
                    (x.Role == UserRole.Staff && x.WarehouseId == currentWarehouseId)
                );
            }

            var users = await query.ToListAsync();
            var dtos = _mapper.Map<IEnumerable<UserDto>>(users).ToList();

            for (int i = 0; i < users.Count; i++)
            {
                dtos[i].WarehouseName = users[i].Warehouse?.Name ?? "Depo Atanmamış";
            }

            return dtos;
        }

        public async Task ToggleUserStatusAsync(Guid id)
        {
            var user = await _userRepository.Where(x => x.Id == id).SingleOrDefaultAsync();
            if (user == null) throw new ClientSideException("Kullanıcı bulunamadı.");

            var currentUserRole = GetCurrentUserRole();
            if (currentUserRole == UserRole.WarehouseManager.ToString())
            {
                if (user.Role == UserRole.SuperAdmin || user.Role == UserRole.WarehouseManager)
                    throw new ClientSideException("Yetki Reddi: Kendi seviyenizdeki veya üstünüzdeki bir hesaba müdahale edemezsiniz.");

                var currentWarehouseId = await GetCurrentWarehouseIdAsync();
                if (user.WarehouseId != currentWarehouseId)
                    throw new ClientSideException("Yetki Reddi: Sadece kendi deponuzdaki personellere müdahale edebilirsiniz.");
            }

            user.IsActive = !user.IsActive;
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();
        }

        #endregion

        #region Profile & Security

        public async Task<UserDto> UpdateProfileAsync(Guid userId, UserProfileUpdateDto profileDto)
        {
            profileDto.FirstName = profileDto.FirstName?.Trim();
            profileDto.LastName = profileDto.LastName?.Trim();

            var user = await _userRepository.Where(x => x.Id == userId && x.IsActive).SingleOrDefaultAsync();
            if (user == null) throw new ClientSideException("Kullanıcı bulunamadı.");

            user.FirstName = profileDto.FirstName;
            user.LastName = profileDto.LastName;
            user.Phone = NormalizePhoneNumber(profileDto.Phone);
            user.AvatarUrl = profileDto.AvatarUrl;
            user.ReceiveEmailNotifications = profileDto.ReceiveEmailNotifications;
            user.ReceiveInAppNotifications = profileDto.ReceiveInAppNotifications;

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<UserDto>(user);
        }

        public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
        {
            var user = await _userRepository.Where(x => x.Id == userId && x.IsActive).SingleOrDefaultAsync();
            if (user == null) throw new ClientSideException("Kullanıcı bulunamadı.");

            if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.PasswordHash))
                throw new ClientSideException("Mevcut şifrenizi hatalı girdiniz.");

            if (BCrypt.Net.BCrypt.Verify(dto.NewPassword, user.PasswordHash))
                throw new ClientSideException("Yeni şifreniz mevcut şifrenizle aynı olamaz.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.CreateAsync(new AuditLogCreateDto
            {
                UserId = userId,
                ActionType = AuditActionType.PasswordChanged,
                TableName = "Users"
            });
        }

        public async Task ResetPasswordAsync(Guid id, string newPassword)
        {
            var user = await _userRepository.Where(x => x.Id == id && x.IsActive).SingleOrDefaultAsync();
            if (user == null) throw new ClientSideException("Şifresi sıfırlanacak aktif kullanıcı bulunamadı.");

            var currentUserRole = GetCurrentUserRole();
            if (currentUserRole == UserRole.WarehouseManager.ToString())
            {
                if (user.Role == UserRole.SuperAdmin || user.Role == UserRole.WarehouseManager)
                    throw new ClientSideException("Yetki Reddi: Kendi seviyenizdeki veya üstünüzdeki bir hesabın şifresini sıfırlayamazsınız.");

                var currentWarehouseId = await GetCurrentWarehouseIdAsync();
                if (user.WarehouseId != currentWarehouseId)
                    throw new ClientSideException("Yetki Reddi: Sadece kendi deponuzdaki personellerin şifresini sıfırlayabilirsiniz.");
            }

            if (BCrypt.Net.BCrypt.Verify(newPassword, user.PasswordHash))
                throw new ClientSideException("Yeni şifre mevcut şifreyle aynı olamaz.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.CreateAsync(new AuditLogCreateDto
            {
                UserId = id,
                ActionType = AuditActionType.PasswordChanged,
                TableName = "Users",
                NewValues = "Şifre sistem yöneticisi tarafından sıfırlandı."
            });
        }

        #endregion

        #region Email Verification Flow

        public async Task RequestEmailChangeAsync(Guid userId, string newEmail)
        {
            var cleanNewEmail = newEmail.Trim().ToLower();

            var user = await _userRepository.Where(x => x.Id == userId && x.IsActive).SingleOrDefaultAsync();
            if (user == null) throw new ClientSideException("Kullanıcı bulunamadı.");

            if (user.Email == cleanNewEmail)
                throw new ClientSideException("Yeni e-posta adresiniz mevcut e-posta adresinizle aynı olamaz.");

            var isEmailExists = await _userRepository.AnyAsync(x => x.Email == cleanNewEmail);
            if (isEmailExists) throw new ClientSideException("Bu e-posta adresi başka bir hesaba kayıtlı.");

            var token = Guid.NewGuid().ToString("N");

            user.PendingNewEmail = cleanNewEmail;
            user.EmailChangeToken = token;
            user.EmailChangeTokenExpires = DateTime.UtcNow.AddHours(2);

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            var clientBaseUrl = _configuration["ClientSettings:BaseUrl"];
            var confirmLink = $"{clientBaseUrl}/confirm-email?token={token}";

            var mailBody = $"Merhaba {user.FirstName},\n\nE-posta adresinizi '{cleanNewEmail}' olarak değiştirmek için bir talep oluşturduk. İşlemi tamamlamak için lütfen aşağıdaki bağlantıya tıklayınız. (Link 2 saat geçerlidir):\n\n{confirmLink}\n\nEğer bu işlemi siz talep etmediyseniz, lütfen sistem yöneticinize başvurunuz ve şifrenizi yenileyiniz.";

            await _emailService.SendEmailAsync(cleanNewEmail, "E-Posta Değişikliği Doğrulaması", mailBody);

            await _auditLogService.CreateAsync(new AuditLogCreateDto
            {
                UserId = user.Id,
                ActionType = AuditActionType.PasswordChanged,
                TableName = "Users",
                NewValues = $"'{cleanNewEmail}' adresine e-posta doğrulama linki gönderildi."
            });
        }

        public async Task ConfirmEmailChangeAsync(string token)
        {

            var user = await _userRepository.Where(x => x.EmailChangeToken == token && x.IsActive).SingleOrDefaultAsync();

            if (user == null || user.EmailChangeTokenExpires < DateTime.UtcNow)
                throw new ClientSideException("Doğrulama linki geçersiz veya süresi dolmuş. Lütfen yeniden talep oluşturun.");

            if (string.IsNullOrWhiteSpace(user.PendingNewEmail))
                throw new ClientSideException("Değiştirilecek bekleyen bir e-posta adresi bulunamadı.");

            var isEmailTaken = await _userRepository.AnyAsync(x => x.Email == user.PendingNewEmail && x.Id != user.Id);
            if (isEmailTaken)
                throw new ClientSideException("Maalesef bu e-posta adresi bu süreçte başka bir hesaba kaydedilmiş.");

            var oldEmail = user.Email;


            user.Email = user.PendingNewEmail;
            user.PendingNewEmail = null;
            user.EmailChangeToken = null;
            user.EmailChangeTokenExpires = null;

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.CreateAsync(new AuditLogCreateDto
            {
                UserId = user.Id,
                ActionType = AuditActionType.PasswordChanged,
                TableName = "Users",
                NewValues = $"E-Posta adresi '{oldEmail}' adresinden '{user.Email}' olarak güncellendi."
            });
        }

        #endregion

        #region Private Helpers

        private string NormalizePhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return string.Empty;
            var cleanPhone = new string(phone.Where(c => char.IsDigit(c) || c == '+').ToArray());
            if (cleanPhone.StartsWith("0")) cleanPhone = "+90" + cleanPhone.Substring(1);
            else if (!cleanPhone.StartsWith("+")) cleanPhone = "+90" + cleanPhone;
            return cleanPhone;
        }

        private string GetCurrentUserRole()
        {
            return _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        private async Task<Guid?> GetCurrentWarehouseIdAsync()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim != null && Guid.TryParse(userIdClaim, out var userId))
            {
                var user = await _userRepository.Where(u => u.Id == userId && u.IsActive).SingleOrDefaultAsync();
                return user?.WarehouseId;
            }

            return null;
        }

        #endregion
    }
}