using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiWarehouse.Service.Services.Interfaces;
using MultiWarehouse.Shared.DTOs;
using MultiWarehouse.Shared.DTOs.AuditLogDtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        /// <summary>Sisteme yeni bir denetim kaydı ekler. (Genellikle sistem tarafından otomatik tetiklenir).</summary>
        [HttpPost]
        public async Task<IActionResult> Create(AuditLogCreateDto createDto)
        {
            var log = await _auditLogService.CreateAsync(createDto);
            return Ok(CustomResponseDto<AuditLogDto>.SuccessResponse(log));
        }

        /// <summary>Belirtilen ID'ye sahip detaylı denetim kaydını (eski/yeni JSON değerleri dahil) getirir.</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var log = await _auditLogService.GetByIdAsync(id);
            return Ok(CustomResponseDto<AuditLogDto>.SuccessResponse(log));
        }

        /// <summary>Sistemdeki tüm denetim izi geçmişini sondan başa doğru getirir.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var logs = await _auditLogService.GetAllAsync();
            return Ok(CustomResponseDto<IEnumerable<AuditLogDto>>.SuccessResponse(logs));
        }

        /// <summary>Sadece belirtilen kullanıcının yaptığı işlemleri listeler. (Şüpheli işlem takibi için).</summary>
        [HttpGet("GetByUserId/{userId}")]
        public async Task<IActionResult> GetByUserId(Guid userId)
        {
            var logs = await _auditLogService.GetAllByUserIdAsync(userId);
            return Ok(CustomResponseDto<IEnumerable<AuditLogDto>>.SuccessResponse(logs));
        }

        /// <summary>Sadece belirtilen tabloda (Örn: "Products") yapılan değişiklikleri listeler.</summary>
        [HttpGet("GetByTableName/{tableName}")]
        public async Task<IActionResult> GetByTableName(string tableName)
        {
            var logs = await _auditLogService.GetAllByTableNameAsync(tableName);
            return Ok(CustomResponseDto<IEnumerable<AuditLogDto>>.SuccessResponse(logs));
        }
    }
}