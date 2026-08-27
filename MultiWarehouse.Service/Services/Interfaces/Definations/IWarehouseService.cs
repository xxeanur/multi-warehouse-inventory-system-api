using MultiWarehouse.Shared.DTOs.WarehouseDtos;
using MultiWarehouse.Shared.Pagination;

namespace MultiWarehouse.Service.Services.Interfaces.Definations
{
    public interface IWarehouseService
    {
        #region Write Operations

        /// <summary>
        /// Sisteme yeni bir depo ekler. Başlangıç doluluk oranı 0'dır. (Sadece SuperAdmin)
        /// </summary>
        Task<WarehouseDto> CreateAsync(WarehouseCreateDto createDto);

        /// <summary>
        /// Mevcut bir deponun bilgilerini günceller. (Sadece SuperAdmin)
        /// </summary>
        Task<WarehouseDto> UpdateAsync(WarehouseUpdateDto updateDto);

        /// <summary>
        /// Belirtilen depoyu sistemden siler (pasife çeker). İçi doluysa silinemez. (Sadece SuperAdmin)
        /// </summary>
        Task RemoveAsync(Guid id);

        #endregion

        #region Read Operations

        /// <summary>
        /// Belirtilen ID'ye sahip depoyu detaylarıyla getirir.
        /// </summary>
        Task<WarehouseDto> GetByIdAsync(Guid id);

        /// <summary>
        /// Sistemdeki tüm aktif depoları listeler.
        /// </summary>
        Task<IEnumerable<WarehouseDto>> GetAllAsync();

        /// <summary>
        /// Sistemdeki depoları sayfalama (Pagination) destekli olarak getirir.
        /// </summary>
        Task<PagedResult<WarehouseDto>> GetPagedAsync(PaginationParams paginationParams);

        #endregion
    }
}