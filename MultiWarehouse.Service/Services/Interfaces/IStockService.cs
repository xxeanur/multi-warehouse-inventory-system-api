using MultiWarehouse.Shared.DTOs.StockDtos;
using MultiWarehouse.Shared.Pagination;

namespace MultiWarehouse.Service.Services.Interfaces
{
    public interface IStockService
    {
        Task<StockDto> CreateAsync(StockCreateDto createDto);
        Task<StockDto> GetByIdAsync(Guid id);
        Task<IEnumerable<StockDto>> GetAllAsync();

        /// <summary>Belirli bir ürüne ait tüm stok noktalarını getirir.</summary>
        Task<IEnumerable<StockDto>> GetAllByProductIdAsync(Guid productId);

        /// <summary>Belirli bir depodaki tüm stokları getirir.</summary>
        Task<IEnumerable<StockDto>> GetAllByWarehouseIdAsync(Guid warehouseId);

        /// <summary>Spesifik bir raftaki tüm stokları getirir.</summary>
        Task<IEnumerable<StockDto>> GetAllByShelfIdAsync(Guid shelfId);

        //pagination
        // Tüm stokları sayfalar
        Task<PagedResult<StockDto>> GetPagedAsync(PaginationParams paginationParams);

        // Belirli bir ürüne ait stokları sayfalar
        Task<PagedResult<StockDto>> GetPagedByProductIdAsync(PaginationParams paginationParams, Guid productId);

        // Belirli bir depodaki stokları sayfalar
        Task<PagedResult<StockDto>> GetPagedByWarehouseIdAsync(PaginationParams paginationParams, Guid warehouseId);

        // Belirli bir raftaki stokları sayfalar
        Task<PagedResult<StockDto>> GetPagedByShelfIdAsync(PaginationParams paginationParams, Guid shelfId);

        Task<StockDto> UpdateAsync(StockUpdateDto updateDto);
        Task RemoveAsync(Guid id);
    }
}