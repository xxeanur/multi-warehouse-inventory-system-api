using MultiWarehouse.Shared.DTOs.InventoryDtos;
using MultiWarehouse.Shared.DTOs.StockMovementDtos;
using MultiWarehouse.Shared.Pagination;

namespace MultiWarehouse.Service.Services.Interfaces.Inventory
{
    public interface IStockMovementService
    {
        #region Read Operations

        /// <summary>
        /// Belirtilen filtrelere göre stok hareketlerini sayfalamalı getirir. (Yetkiye göre filtrelenir)
        /// </summary>
        Task<PagedResult<StockMovementListDto>> GetFilteredPagedAsync(StockMovementFilterDto filterDto, PaginationParams paginationParams);

        /// <summary>
        /// Çekmece (Drawer) için tekil detay getirir.
        /// </summary>
        Task<StockMovementDetailDto> GetDetailByIdAsync(Guid id);

        /// <summary>
        /// Belirli bir ürünün tüm hareket geçmişini kronolojik ve sayfalamalı getirir. (Yetkiye göre filtrelenir)
        /// </summary>
        Task<PagedResult<StockMovementListDto>> GetByProductIdAsync(Guid productId, PaginationParams paginationParams);

        /// <summary>
        /// Belirli bir rafa ait stok hareket geçmişini sayfalamalı getirir. (Yetkiye göre filtrelenir)
        /// </summary>
        Task<PagedResult<StockMovementListDto>> GetByShelfIdAsync(Guid shelfId, PaginationParams paginationParams);

        /// <summary>
        /// Belirli bir belgeye ait tüm hareket kayıtlarını sayfalamalı getirir. (Yetkiye göre filtrelenir)
        /// </summary>
        Task<PagedResult<StockMovementListDto>> GetByDocumentIdAsync(Guid documentId, PaginationParams paginationParams);

        #endregion
    }
}