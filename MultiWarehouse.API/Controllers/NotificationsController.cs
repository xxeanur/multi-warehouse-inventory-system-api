using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiWarehouse.Service.Services.Interfaces.Notification;
using MultiWarehouse.Shared.DTOs;
using MultiWarehouse.Shared.DTOs.NotificationDtos;
using MultiWarehouse.Shared.Pagination;

namespace MultiWarehouse.API.Controllers
{
    /// <summary>
    /// Sistem içi bildirimlerin yönetildiği API uç noktasıdır.
    /// Güvenlik ve veri izolasyonu servis katmanında (IDOR Koruması ile) yönetilmektedir.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        #region Read Operations

        /// <summary>
        /// İlgili bildirimin detaylarını getirir. Sadece bildirimin sahibi erişebilir.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var notification = await _notificationService.GetByIdAsync(id);
            return Ok(CustomResponseDto<NotificationDto>.SuccessResponse(notification));
        }

        /// <summary>
        /// Kullanıcının tüm aktif bildirimlerini listeler.
        /// </summary>
        [HttpGet("my-notifications")]
        public async Task<IActionResult> GetMyNotifications()
        {
            var notifications = await _notificationService.GetAllByUserIdAsync();
            return Ok(CustomResponseDto<IEnumerable<NotificationDto>>.SuccessResponse(notifications));
        }

        /// <summary>
        /// Kullanıcının bildirim geçmişini sayfalamalı (Pagination) olarak getirir.
        /// </summary>
        [HttpGet("my-notifications/paged")]
        public async Task<IActionResult> GetMyPagedNotifications([FromQuery] PaginationParams paginationParams)
        {
            var pagedNotifications = await _notificationService.GetPagedByUserIdAsync(paginationParams);
            return Ok(CustomResponseDto<PagedResult<NotificationDto>>.SuccessResponse(pagedNotifications));
        }

        /// <summary>
        /// Kullanıcının henüz okumadığı bildirim sayısını döndürür.
        /// </summary>
        [HttpGet("my-unread-count")]
        public async Task<IActionResult> GetMyUnreadCount()
        {
            var count = await _notificationService.GetUnreadCountAsync();
            return Ok(CustomResponseDto<int>.SuccessResponse(count));
        }

        #endregion

        #region Write Operations

        /// <summary>
        /// Belirtilen bildirimi "Okundu" olarak işaretler.
        /// </summary>
        [HttpPatch("{id}/mark-as-read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return Ok(CustomResponseDto.SuccessResponse());
        }

        /// <summary>
        /// Kullanıcıya ait tüm okunmamış bildirimleri tek hamlede "Okundu" olarak işaretler.
        /// </summary>
        [HttpPatch("mark-all-as-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            await _notificationService.MarkAllAsReadAsync();
            return Ok(CustomResponseDto.SuccessResponse());
        }

        /// <summary>
        /// Belirtilen bildirimi sistemden siler (Soft Delete).
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Remove(Guid id)
        {
            await _notificationService.RemoveAsync(id);
            return Ok(CustomResponseDto.SuccessResponse());
        }

        #endregion
    }
}