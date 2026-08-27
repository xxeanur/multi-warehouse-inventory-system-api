using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MultiWarehouse.Entity.Entities.Identity;
using MultiWarehouse.Service.Exceptions;
using MultiWarehouse.Service.Repositories.Interfaces;
using MultiWarehouse.Service.Services.Interfaces.Identity;
using MultiWarehouse.Service.Services.Interfaces.Notification;
using MultiWarehouse.Shared.DTOs.NotificationDtos;
using MultiWarehouse.Shared.Pagination;
using System.Security.Claims;
using NotificationEntity = MultiWarehouse.Entity.Entities.Notification.Notification;

namespace MultiWarehouse.Service.Services.Implementations.Notification
{
    public class NotificationService : INotificationService
    {
        #region Dependencies

        private readonly IGenericRepository<NotificationEntity> _notificationRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NotificationService(
            IGenericRepository<NotificationEntity> notificationRepository,
            IGenericRepository<User> userRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IEmailService emailService,
            IHttpContextAccessor httpContextAccessor)
        {
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region Write Operations

        public async Task<NotificationDto?> CreateAsync(NotificationCreateDto createDto)
        {
            var user = await _userRepository.Where(u => u.Id == createDto.UserId && u.IsActive).SingleOrDefaultAsync();
            if (user == null) throw new ClientSideException("Bildirim gönderilmek istenen kullanıcı sistemde bulunamadı.");

            NotificationEntity? notification = null;

            if (user.ReceiveInAppNotifications)
            {
                notification = _mapper.Map<NotificationEntity>(createDto);
                notification.IsRead = false;
                await _notificationRepository.AddAsync(notification);
                await _unitOfWork.SaveChangesAsync();
            }

            if (user.ReceiveEmailNotifications)
            {
                try
                {
                    await _emailService.SendEmailAsync(user.Email, createDto.Title, createDto.Message);
                }
                catch (Exception)
                {

                }
            }

            return notification != null ? _mapper.Map<NotificationDto>(notification) : null;
        }

        public async Task MarkAsReadAsync(Guid id)
        {
            var userId = GetCurrentUserId();
            var notification = await _notificationRepository.Where(n => n.Id == id && n.UserId == userId && n.IsActive).SingleOrDefaultAsync();
            if (notification == null) throw new ClientSideException("İşlem yapılmak istenen bildirim bulunamadı veya yetkiniz yok.");

            notification.IsRead = true;
            _notificationRepository.Update(notification);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync()
        {
            var userId = GetCurrentUserId();
            var unreadNotifications = await _notificationRepository
                .Where(n => n.UserId == userId && !n.IsRead && n.IsActive)
                .ToListAsync();

            if (unreadNotifications.Any())
            {
                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                    _notificationRepository.Update(notification);
                }
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task RemoveAsync(Guid id)
        {
            var userId = GetCurrentUserId();
            var notification = await _notificationRepository.Where(n => n.Id == id && n.UserId == userId && n.IsActive).SingleOrDefaultAsync();
            if (notification == null) throw new ClientSideException("Silinmek istenen bildirim bulunamadı veya yetkiniz yok.");

            notification.IsActive = false;
            _notificationRepository.Update(notification);
            await _unitOfWork.SaveChangesAsync();
        }

        #endregion

        #region Read Operations

        public async Task<NotificationDto> GetByIdAsync(Guid id)
        {
            var userId = GetCurrentUserId();
            var notification = await _notificationRepository.Where(n => n.Id == id && n.UserId == userId && n.IsActive).SingleOrDefaultAsync();
            if (notification == null) throw new ClientSideException("Bildirim bulunamadı veya bu bildirimi görüntüleme yetkiniz yok.");
            return _mapper.Map<NotificationDto>(notification);
        }

        public async Task<IEnumerable<NotificationDto>> GetAllByUserIdAsync()
        {
            var userId = GetCurrentUserId();
            var notifications = await _notificationRepository
                .Where(n => n.UserId == userId && n.IsActive)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<NotificationDto>>(notifications);
        }

        public async Task<PagedResult<NotificationDto>> GetPagedByUserIdAsync(PaginationParams paginationParams)
        {
            var userId = GetCurrentUserId();
            var pagedEntities = await _notificationRepository.GetPagedAsync(
                paginationParams,
                filter: n => n.IsActive && n.UserId == userId,
                orderBy: q => q.OrderByDescending(n => n.CreatedDate)
            );

            return _mapper.Map<PagedResult<NotificationDto>>(pagedEntities);
        }

        public async Task<int> GetUnreadCountAsync()
        {
            var userId = GetCurrentUserId();
            return await _notificationRepository
                .Where(n => n.UserId == userId && !n.IsRead && n.IsActive)
                .CountAsync();
        }

        #endregion

        #region Private Helpers

        private Guid GetCurrentUserId()
        {
            var userIdString = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdString, out var userId))
                return userId;

            throw new UnauthorizedAccessException("Kullanıcı kimliği doğrulanamadı.");
        }

        #endregion
    }
}