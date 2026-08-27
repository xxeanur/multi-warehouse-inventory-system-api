using MultiWarehouse.Shared.DTOs.DocumentDtos.OutboundDtos;
using MultiWarehouse.Shared.Pagination;

namespace MultiWarehouse.Service.Services.Interfaces.Documents
{
    public interface IOutboundOrderService
    {
        #region Write Operations

        /// <summary>
        /// Yeni çıkış fişi oluşturur (Durumu: Pending). Stokları rezerve eder.
        /// </summary>
        Task<Guid> CreateAsync(OutboundOrderCreateDto createDto);

        /// <summary>
        /// Fişi ve satırlarını raflardan toplayarak onaylar (Completed). Rezerveyi ve stoğu düşer.
        /// </summary>
        Task ApproveAsync(OutboundOrderApproveDto approveDto);

        /// <summary>
        /// Bekleyen (Pending) fişi iptal eder (Cancelled). Rezerve stokları nokta atışı serbest bırakır.
        /// </summary>
        Task CancelAsync(OutboundOrderCancelDto cancelDto);

        #endregion

        #region Read Operations

        /// <summary>
        /// Tüm çıkış fişlerini listeler (RLS Korumalı).</summary>
        Task<IEnumerable<OutboundOrderListDto>> GetAllAsync();

        /// <summary>
        /// Sayfalamalı listeleme yapar (RLS Korumalı).
        /// </summary>
        Task<PagedResult<OutboundOrderListDto>> GetPagedAsync(PaginationParams paginationParams);

        /// <summary>
        /// Spesifik bir depoya ait çıkış fişlerini listeler (RLS Korumalı).
        /// </summary>
        Task<IEnumerable<OutboundOrderListDto>> GetAllByWarehouseIdAsync(Guid warehouseId);

        /// <summary>
        /// Fişin tüm satırları ve detaylarıyla getirir (RLS Korumalı).
        /// </summary>
        Task<OutboundOrderDetailDto> GetByIdAsync(Guid id);

        #endregion
    }
}