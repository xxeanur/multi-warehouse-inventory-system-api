using MultiWarehouse.Shared.DTOs.StockMovementDtos;
using MultiWarehouse.Shared.Pagination;

namespace MultiWarehouse.Service.Services.Interfaces
{
    public interface IStockMovementService
    {
        Task<StockMovementDto> CreateAsync(StockMovementCreateDto createDto);
        Task<StockMovementDto> GetByIdAsync(Guid id);
        Task<IEnumerable<StockMovementDto>> GetAllAsync();
        Task<IEnumerable<StockMovementDto>> GetAllByProductIdAsync(Guid productId);
        Task<IEnumerable<StockMovementDto>> GetAllByWarehouseIdAsync(Guid warehouseId);
        //pagination
        // Tüm hareketleri sayfalar (Genel Rapor)
        Task<PagedResult<StockMovementDto>> GetPagedAsync(PaginationParams paginationParams);

        // Belirli bir ürüne ait hareketleri sayfalar (Ürün Tarihçesi)
        Task<PagedResult<StockMovementDto>> GetPagedByProductIdAsync(PaginationParams paginationParams, Guid productId);

        // Belirli bir depoyu etkileyen (Kaynak veya Hedef) hareketleri sayfalar
        Task<PagedResult<StockMovementDto>> GetPagedByWarehouseIdAsync(PaginationParams paginationParams, Guid warehouseId);
        Task<StockMovementDto> UpdateAsync(StockMovementUpdateDto updateDto);
        Task RemoveAsync(Guid id);
    }
}