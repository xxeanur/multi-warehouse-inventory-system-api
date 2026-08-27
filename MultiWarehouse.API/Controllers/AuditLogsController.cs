using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiWarehouse.Service.Services.Interfaces.Common;
using MultiWarehouse.Shared.DTOs;
using MultiWarehouse.Shared.DTOs.AuditLogDtos;
using MultiWarehouse.Shared.Pagination;

namespace MultiWarehouse.API.Controllers
{
    /// <summary>
    /// Sistemin güvenlik duvarıdır. Hangi kullanıcının hangi veriyi ne zaman değiştirdiğini gösterir.
    /// Kısıtlama: Bu uç noktalara SADECE SuperAdmin rolüne sahip kişiler erişebilir.
    /// </summary>
    [Authorize(Roles = "SuperAdmin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogsController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        #region Write Operations

        /// <summary>
        /// Sisteme yeni bir denetim kaydı ekler. Genellikle sistem/middleware tarafından otomatik tetiklenir.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(AuditLogCreateDto createDto)
        {
            var log = await _auditLogService.CreateAsync(createDto);
            return Ok(CustomResponseDto<AuditLogDto>.SuccessResponse(log));
        }

        // DİKKAT: Logların güvenliği için Update ve Delete endpointleri kasıtlı olarak açık bırakılmamıştır.

        #endregion

        #region Read Operations

        /// <summary>
        /// Belirtilen ID'ye sahip detaylı denetim kaydını (eski/yeni JSON değerleri dahil) getirir.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var log = await _auditLogService.GetByIdAsync(id);
            return Ok(CustomResponseDto<AuditLogDto>.SuccessResponse(log));
        }

        /// <summary>
        /// Sistemdeki tüm denetim izi geçmişini sondan başa doğru getirir.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var logs = await _auditLogService.GetAllAsync();
            return Ok(CustomResponseDto<IEnumerable<AuditLogDto>>.SuccessResponse(logs));
        }

        /// <summary>
        /// Sadece belirtilen kullanıcının yaptığı işlemleri listeler. Şüpheli işlem takibi için kullanılır.
        /// </summary>
        [HttpGet("GetByUserId/{userId}")]
        public async Task<IActionResult> GetByUserId(Guid userId)
        {
            var logs = await _auditLogService.GetAllByUserIdAsync(userId);
            return Ok(CustomResponseDto<IEnumerable<AuditLogDto>>.SuccessResponse(logs));
        }

        /// <summary>
        /// Sadece belirtilen tabloda (Örn: "Products") yapılan değişiklikleri listeler.
        /// </summary>
        [HttpGet("GetByTableName/{tableName}")]
        public async Task<IActionResult> GetByTableName(string tableName)
        {
            var logs = await _auditLogService.GetAllByTableNameAsync(tableName);
            return Ok(CustomResponseDto<IEnumerable<AuditLogDto>>.SuccessResponse(logs));
        }

        /// <summary>
        /// Tüm sistem loglarını en yeniden eskiye doğru sayfalama destekli olarak getirir.
        /// </summary>
        [HttpGet("Paged")]
        public async Task<IActionResult> GetPaged([FromQuery] PaginationParams paginationParams)
        {
            var pagedLogs = await _auditLogService.GetPagedAsync(paginationParams);
            return Ok(CustomResponseDto<PagedResult<AuditLogDto>>.SuccessResponse(pagedLogs));
        }

        /// <summary>
        /// Belirli bir kullanıcının işlem geçmişini en yeniden eskiye doğru sayfalayarak getirir.
        /// </summary>
        [HttpGet("PagedByUser/{userId}")]
        public async Task<IActionResult> GetPagedByUser([FromQuery] PaginationParams paginationParams, Guid userId)
        {
            var pagedLogs = await _auditLogService.GetPagedByUserIdAsync(paginationParams, userId);
            return Ok(CustomResponseDto<PagedResult<AuditLogDto>>.SuccessResponse(pagedLogs));
        }

        /// <summary>
        /// Belirli bir tablo üzerinde yapılan değişiklikleri sayfalayarak getirir.
        /// </summary>
        [HttpGet("PagedByTable/{tableName}")]
        public async Task<IActionResult> GetPagedByTable([FromQuery] PaginationParams paginationParams, string tableName)
        {
            var pagedLogs = await _auditLogService.GetPagedByTableNameAsync(paginationParams, tableName);
            return Ok(CustomResponseDto<PagedResult<AuditLogDto>>.SuccessResponse(pagedLogs));
        }

        #endregion
    }
}