using MultiWarehouse.Shared.DTOs.NotificationDtos;
using MultiWarehouse.Shared.Pagination;

namespace MultiWarehouse.Service.Services.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationDto> CreateAsync(NotificationCreateDto createDto);
        Task<NotificationDto> GetByIdAsync(Guid id);

        /// <summary>Belirli bir kullanıcıya ait tüm aktif bildirimleri getirir.</summary>
        Task<IEnumerable<NotificationDto>> GetAllByUserIdAsync(Guid userId);

        //pagination
        // Belirli bir kullanıcıya ait bildirimleri sayfalar (Bildirim Geçmişi Ekranı İçin)
        Task<PagedResult<NotificationDto>> GetPagedByUserIdAsync(PaginationParams paginationParams, Guid userId);

        /// <summary>Kullanıcının henüz okumadığı bildirimlerin sayısını döndürür (Zil ikonu için).</summary>
        Task<int> GetUnreadCountAsync(Guid userId);

        /// <summary>Tek bir bildirimi okundu olarak işaretler.</summary>
        Task MarkAsReadAsync(Guid id);

        /// <summary>Kullanıcıya ait tüm okunmamış bildirimleri tek seferde okundu olarak işaretler.</summary>
        Task MarkAllAsReadAsync(Guid userId);

        Task RemoveAsync(Guid id);
    }
}