using MultiWarehouse.Shared.DTOs.StockDtos;
using MultiWarehouse.Shared.Pagination;

namespace MultiWarehouse.Service.Services.Interfaces.Inventory
{
    /// <summary>
    /// Depolardaki anlık stok miktarlarını raf, ürün ve depo bazlı sorgulamak için kullanılan servis.
    /// </summary>
    public interface IStockService
    {
        #region Read Operations

        /// <summary>
        /// Belirtilen benzersiz ID'ye sahip stok kaydını getirir.
        /// </summary>
        Task<StockDto> GetByIdAsync(Guid id);

        /// <summary>
        /// Sistemdeki tüm aktif stok durumlarını listeler.
        /// </summary>
        Task<IEnumerable<StockDto>> GetAllAsync();

        /// <summary>
        /// Belirli bir ürünün sistemdeki tüm stok konumlarını ve miktarlarını getirir.
        /// </summary>
        Task<IEnumerable<StockDto>> GetAllByProductIdAsync(Guid productId);

        /// <summary>
        /// Belirli bir depoda bulunan tüm ürünlerin stok durumlarını listeler.
        /// </summary>
        Task<IEnumerable<StockDto>> GetAllByWarehouseIdAsync(Guid warehouseId);

        /// <summary>
        /// Belirli bir rafta bulunan tüm stokları listeler.
        /// </summary>
        Task<IEnumerable<StockDto>> GetAllByShelfIdAsync(Guid shelfId);

        // Pagination

        /// <summary>
        /// Sistemdeki tüm stokları sayfalama (pagination) formatında getirir.
        /// </summary>
        Task<PagedResult<StockDto>> GetPagedAsync(PaginationParams paginationParams);

        /// <summary>
        /// Belirli bir ürünün stoklarını sayfalayarak listeler.
        /// </summary>
        Task<PagedResult<StockDto>> GetPagedByProductIdAsync(PaginationParams paginationParams, Guid productId);

        /// <summary>
        /// Belirli bir depodaki stokları sayfalayarak listeler.
        /// </summary>
        Task<PagedResult<StockDto>> GetPagedByWarehouseIdAsync(PaginationParams paginationParams, Guid warehouseId);

        /// <summary>
        /// Belirli bir raftaki stokları sayfalayarak listeler.
        /// </summary>
        Task<PagedResult<StockDto>> GetPagedByShelfIdAsync(PaginationParams paginationParams, Guid shelfId);

        #endregion
    }
}