using MultiWarehouse.Shared.DTOs.AuditLogDtos;
using MultiWarehouse.Shared.Pagination;

namespace MultiWarehouse.Service.Services.Interfaces.Common
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

        // Tüm logları sayfalar
        Task<PagedResult<AuditLogDto>> GetPagedAsync(PaginationParams paginationParams);

        // Belirli bir kullanıcının yaptığı işlemleri sayfalar
        Task<PagedResult<AuditLogDto>> GetPagedByUserIdAsync(PaginationParams paginationParams, Guid userId);

        // Sadece belirli bir tabloda (Örn: "Products") yapılan işlemleri sayfalar
        Task<PagedResult<AuditLogDto>> GetPagedByTableNameAsync(PaginationParams paginationParams, string tableName);

        Task<IEnumerable<AuditLogDto>> GetRecentSecurityLogsByUserIdAsync(Guid userId, int count = 10);
    }
}