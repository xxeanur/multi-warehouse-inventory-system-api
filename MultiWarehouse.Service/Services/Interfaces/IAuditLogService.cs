// MultiWarehouse.Service/Services/Interfaces/IAuditLogService.cs
using MultiWarehouse.Shared.DTOs.AuditLogDtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiWarehouse.Service.Services.Interfaces
{
    public interface IAuditLogService
    {
        /// <summary>Sisteme yeni bir denetim izi kaydeder.</summary>
        Task<AuditLogDto> CreateAsync(AuditLogCreateDto createDto);

        Task<AuditLogDto> GetByIdAsync(Guid id);

        /// <summary>Sistemdeki tüm denetim izlerini kronolojik sırayla getirir.</summary>
        Task<IEnumerable<AuditLogDto>> GetAllAsync();

        /// <summary>Spesifik bir kullanıcının sistemde yaptığı tüm işlemleri listeler.</summary>
        Task<IEnumerable<AuditLogDto>> GetAllByUserIdAsync(Guid userId);

        /// <summary>Belirli bir tablo (Örn: Products) üzerinde yapılan tüm işlemleri listeler.</summary>
        Task<IEnumerable<AuditLogDto>> GetAllByTableNameAsync(string tableName);
    }
}