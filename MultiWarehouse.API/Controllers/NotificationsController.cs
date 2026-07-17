using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiWarehouse.Service.Services.Interfaces;
using MultiWarehouse.Shared.DTOs;
using MultiWarehouse.Shared.DTOs.NotificationDtos;
using MultiWarehouse.Shared.Pagination;

namespace MultiWarehouse.API.Controllers
{
    /// <summary>
    /// Kullanıcılara giden uygulama içi bildirimleri ve okunma durumlarını yöneten API.
    /// </summary>
    [Authorize] // Sisteme giriş yapmış herkes kendi bildirimlerini görebilmeli
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>Sistem tarafından kullanıcıya yeni bir bildirim oluşturur.</summary>
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,WarehouseManager")] // Genelde sadece sistem veya yetkililer bildirim atar
        public async Task<IActionResult> Create(NotificationCreateDto createDto)
        {
            var notification = await _notificationService.CreateAsync(createDto);
            return Ok(CustomResponseDto<NotificationDto>.SuccessResponse(notification));
        }

        /// <summary>Belirtilen kullanıcıya ait tüm bildirimleri (okunmuş/okunmamış) listeler.</summary>
        [HttpGet("GetByUserId/{userId}")]
        public async Task<IActionResult> GetByUserId(Guid userId)
        {
            var notifications = await _notificationService.GetAllByUserIdAsync(userId);
            return Ok(CustomResponseDto<IEnumerable<NotificationDto>>.SuccessResponse(notifications));
        }

        /// <summary>
        /// Sadece belirtilen kullanıcıya ait bildirimleri sayfalayarak getirir.
        /// Örnek: GET /api/Notifications/PagedByUser/12345-abcde...?pageNumber=1&pageSize=10
        /// </summary>
        [HttpGet("PagedByUser/{userId}")]
        public async Task<IActionResult> GetPagedByUser([FromQuery] PaginationParams paginationParams, Guid userId)
        {
            var pagedNotifications = await _notificationService.GetPagedByUserIdAsync(paginationParams, userId);

            return Ok(CustomResponseDto<PagedResult<NotificationDto>>.SuccessResponse(pagedNotifications));
        }

        /// <summary>Kullanıcının henüz okumadığı bildirim adedini getirir (Zil ikonu badge'i için).</summary>
        [HttpGet("GetUnreadCount/{userId}")]
        public async Task<IActionResult> GetUnreadCount(Guid userId)
        {
            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(CustomResponseDto<int>.SuccessResponse(count));
        }

        /// <summary>Spesifik bir bildirimi okundu olarak işaretler.</summary>
        [HttpPatch("MarkAsRead/{id}")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return Ok(CustomResponseDto.SuccessResponse());
        }

        /// <summary>Kullanıcıya ait tüm okunmamış bildirimleri tek seferde okundu olarak işaretler.</summary>
        [HttpPatch("MarkAllAsRead/{userId}")]
        public async Task<IActionResult> MarkAllAsRead(Guid userId)
        {
            await _notificationService.MarkAllAsReadAsync(userId);
            return Ok(CustomResponseDto.SuccessResponse());
        }

        /// <summary>Bildirimi siler (Pasife çeker).</summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Remove(Guid id)
        {
            await _notificationService.RemoveAsync(id);
            return Ok(CustomResponseDto.SuccessResponse());
        }
    }
}