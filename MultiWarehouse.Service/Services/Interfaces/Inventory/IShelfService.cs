using MultiWarehouse.Shared.DTOs.ShelfDtos;
using MultiWarehouse.Shared.Pagination;

namespace MultiWarehouse.Service.Services.Interfaces.Inventory
{
    public interface IShelfService
    {
        #region Write Operations

        /// <summary>
        /// Bloğa yeni bir raf ekler. (SuperAdmin veya Kendi Deposundaki Manager)
        /// </summary>
        Task<ShelfDto> CreateAsync(ShelfCreateDto createDto);

        /// <summary>
        /// Mevcut bir rafın özelliklerini günceller. (SuperAdmin veya Kendi Deposundaki Manager)
        /// </summary>
        Task<ShelfDto> UpdateAsync(ShelfUpdateDto updateDto);

        /// <summary>
        /// Belirtilen rafı pasif duruma çeker. İçi dolu raflar silinemez. (SuperAdmin veya Kendi Deposundaki Manager)
        /// </summary>
        Task RemoveAsync(Guid id);

        #endregion

        #region Read Operations

        /// <summary>
        /// Belirtilen ID'ye sahip rafı getirir. (Kullanıcı sadece yetkili olduğu depodakini görebilir)
        /// </summary>
        Task<ShelfDto> GetByIdAsync(Guid id);

        /// <summary>
        /// Tüm aktif rafları listeler. (Manager ve Staff sadece kendi deposuna ait rafları görür)
        /// </summary>
        Task<IEnumerable<ShelfDto>> GetAllAsync();

        /// <summary>
        /// Sistemdeki tüm aktif rafları sayfalama altyapısı ile getirir. (Manager ve Staff için filtrelenir)
        /// </summary>
        Task<PagedResult<ShelfDto>> GetPagedAsync(PaginationParams paginationParams);

        /// <summary>
        /// Belirli bir depo bloğuna (Zone) ait tüm rafları getirir.
        /// </summary>
        Task<IEnumerable<ShelfDto>> GetAllByZoneIdAsync(Guid zoneId);

        /// <summary>
        /// Belirli bir bloğa (Zone) ait rafları sayfalayarak getirir.
        /// </summary>
        Task<PagedResult<ShelfDto>> GetPagedByZoneIdAsync(PaginationParams paginationParams, Guid zoneId);

        #endregion
    }
}