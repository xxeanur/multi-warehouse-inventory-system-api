using MultiWarehouse.Shared.DTOs.NotificationDtos;
using MultiWarehouse.Shared.Pagination;

namespace MultiWarehouse.Service.Services.Interfaces.Notification
{
    public interface INotificationService
    {
        #region Write Operations

        /// <summary>Yeni bir bildirim oluşturur. Gerekirse e-posta tetikler.</summary>
        Task<NotificationDto?> CreateAsync(NotificationCreateDto createDto);

        /// <summary>Tek bir bildirimi okundu olarak işaretler.</summary>
        Task MarkAsReadAsync(Guid id);

        /// <summary>Kullanıcıya ait tüm okunmamış bildirimleri okundu olarak işaretler.</summary>
        Task MarkAllAsReadAsync();

        /// <summary>Bildirimi siler (Pasife çeker).</summary>
        Task RemoveAsync(Guid id);

        #endregion

        #region Read Operations

        /// <summary>Bildirim detayını getirir. Sadece bildirim sahibi görebilir.</summary>
        Task<NotificationDto> GetByIdAsync(Guid id);

        /// <summary>Giriş yapmış kullanıcıya ait tüm aktif bildirimleri getirir.</summary>
        Task<IEnumerable<NotificationDto>> GetAllByUserIdAsync();

        /// <summary>Kullanıcının henüz okumadığı bildirimlerin sayısını döndürür (Zil ikonu için).</summary>
        Task<int> GetUnreadCountAsync();

        /// <summary>Kullanıcıya ait bildirimleri sayfalamalı olarak getirir.</summary>
        Task<PagedResult<NotificationDto>> GetPagedByUserIdAsync(PaginationParams paginationParams);

        #endregion
    }
}