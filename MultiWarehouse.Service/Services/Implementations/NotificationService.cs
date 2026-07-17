using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MultiWarehouse.Entity.Entities;
using MultiWarehouse.Service.Context;
using MultiWarehouse.Service.Exceptions;
using MultiWarehouse.Service.Repositories.Interfaces;
using MultiWarehouse.Service.Services.Interfaces;
using MultiWarehouse.Shared.DTOs.NotificationDtos;
using MultiWarehouse.Shared.Pagination;

namespace MultiWarehouse.Service.Services.Implementations
{
    /// <summary>
    /// Sistem içi bildirimlerin oluşturulması, listelenmesi ve okunma durumlarının (Read/Unread) yönetilmesini sağlayan servis.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly IGenericRepository<Notification> _notificationRepository;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public NotificationService(IGenericRepository<Notification> notificationRepository, AppDbContext context, IMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Kullanıcıya yeni bir sistem bildirimi (Alarm, Bilgi, Uyarı vb.) gönderir.
        /// </summary>
        public async Task<NotificationDto> CreateAsync(NotificationCreateDto createDto)
        {
            var userExists = await _context.Set<User>().AnyAsync(u => u.Id == createDto.UserId && u.IsActive);
            if (!userExists) throw new ClientSideException("Bildirim gönderilmek istenen kullanıcı sistemde bulunamadı.");

            var notification = _mapper.Map<Notification>(createDto);
            notification.IsRead = false; // Yeni bildirim varsayılan olarak okunmamıştır.

            await _notificationRepository.AddAsync(notification);
            await _context.SaveChangesAsync();

            return _mapper.Map<NotificationDto>(notification);
        }

        /// <summary>
        /// ID'si verilen bildirimin detaylarını getirir.
        /// </summary>
        public async Task<NotificationDto> GetByIdAsync(Guid id)
        {
            var notification = await _notificationRepository.Where(n => n.Id == id && n.IsActive).SingleOrDefaultAsync();
            if (notification == null) throw new ClientSideException("Bildirim bulunamadı.");
            return _mapper.Map<NotificationDto>(notification);
        }

        /// <summary>
        /// İlgili kullanıcıya ait tüm bildirimleri tarihe göre yeniden eskiye doğru sıralayarak getirir.
        /// </summary>
        public async Task<IEnumerable<NotificationDto>> GetAllByUserIdAsync(Guid userId)
        {
            var notifications = await _notificationRepository
                .Where(n => n.UserId == userId && n.IsActive)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<NotificationDto>>(notifications);
        }

        //pagination 
        /// <summary>
        /// Kullanıcıya ait bildirimleri sayfalayarak getirir.
        /// (Örn: Bildirimler sayfasına girildiğinde 20'şer 20'şer yüklemek için)
        /// </summary>
        public async Task<PagedResult<NotificationDto>> GetPagedByUserIdAsync(PaginationParams paginationParams, Guid userId)
        {
            var pagedEntities = await _notificationRepository.GetPagedAsync(
                paginationParams,
                filter: n => n.IsActive && n.UserId == userId
            );

            // AutoMapper Open Generics sağ olsun, tek satırda tertemiz dönüşüm!
            return _mapper.Map<PagedResult<NotificationDto>>(pagedEntities);
        }
        /// <summary>
        /// Frontend'de zil (bell) ikonunun üzerinde gösterilecek olan "Okunmamış" bildirim sayısını hesaplar.
        /// </summary>
        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _notificationRepository
                .Where(n => n.UserId == userId && !n.IsRead && n.IsActive)
                .CountAsync();
        }

        /// <summary>
        /// Kullanıcının tıkladığı spesifik bir bildirimi "Okundu" olarak günceller.
        /// </summary>
        public async Task MarkAsReadAsync(Guid id)
        {
            var notification = await _notificationRepository.Where(n => n.Id == id && n.IsActive).SingleOrDefaultAsync();
            if (notification == null) throw new ClientSideException("İşlem yapılmak istenen bildirim bulunamadı.");

            notification.IsRead = true;
            _notificationRepository.Update(notification);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Kullanıcı "Tümünü Okundu İşaretle" butonuna bastığında ait olduğu tüm okunmamış bildirimleri günceller.
        /// </summary>
        public async Task MarkAllAsReadAsync(Guid userId)
        {
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
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Bildirimi sistemden soft delete yöntemiyle siler.
        /// </summary>
        public async Task RemoveAsync(Guid id)
        {
            var notification = await _notificationRepository.Where(n => n.Id == id && n.IsActive).SingleOrDefaultAsync();
            if (notification == null) throw new ClientSideException("Silinmek istenen bildirim bulunamadı.");

            notification.IsActive = false;
            _notificationRepository.Update(notification);
            await _context.SaveChangesAsync();
        }
    }
}