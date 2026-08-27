using MultiWarehouse.Shared.DTOs.DocumentDtos.InboundDtos;
using MultiWarehouse.Shared.Pagination;

namespace MultiWarehouse.Service.Services.Interfaces.Documents
{
    public interface IInboundOrderService
    {
        #region Write Operations

        /// <summary>
        /// Yeni fiş oluşturur (Durumu: Pending). Stoklar etkilenmez.
        /// </summary>
        Task<Guid> CreateAsync(InboundOrderCreateDto createDto);

        /// <summary>
        /// Kapıdaki sayımı doğrular ve fişi onaylar (Durumu: Approved). Raflama işlemini beklemesi için Putaway'e paslar.
        /// </summary>
        Task ApproveAsync(InboundOrderApproveDto approveDto);

        /// <summary>
        /// Fişi iptal eder (Durumu: Cancelled). Tamamlanmış (Completed) belgeler iptal edilemez.
        /// </summary>
        Task CancelAsync(Guid inboundOrderId);

        #endregion

        #region Read Operations

        /// <summary>
        /// Tüm fişleri başlık bilgileriyle (List DTO) sayfalamasız getirir. (RLS ile filtrelenir)
        /// </summary>
        Task<IEnumerable<InboundOrderListDto>> GetAllAsync();

        /// <summary>Fişleri sayfalamalı listeler. (RLS ile filtrelenir)</summary>
        Task<PagedResult<InboundOrderListDto>> GetPagedAsync(PaginationParams paginationParams);

        /// <summary>
        /// Spesifik bir depoya ait fişleri getirir. (RLS ile filtrelenir)
        /// </summary>
        Task<IEnumerable<InboundOrderListDto>> GetAllByWarehouseIdAsync(Guid warehouseId);

        /// <summary>
        /// Fişin tüm satırları ve detaylarıyla getirir. (RLS ile filtrelenir)
        /// </summary>
        Task<InboundOrderDetailDto> GetByIdAsync(Guid id);

        #endregion
    }
}