using MultiWarehouse.Shared.DTOs.DocumentDtos.TransferDtos;
using MultiWarehouse.Shared.Pagination;

namespace MultiWarehouse.Service.Services.Interfaces.Documents
{
    public interface ITransferOrderService
    {
        #region Write Operations

        /// <summary>Yeni transfer fişi oluşturur (Pending). Kaynak depoda rekor tahsis ve rezervasyon yapar.</summary>
        Task<Guid> CreateAsync(TransferOrderCreateDto createDto);

        /// <summary>Beklemedeki transferi yola çıkarır (InTransit). Kaynak depodan fiziksel düşüm yapar.</summary>
        Task DispatchAsync(TransferOrderDispatchDto dispatchDto);

        /// <summary>Yoldaki transferi teslim alır (Completed). Hedef depoya stok ekler ve raf kapasitelerini günceller.</summary>
        Task ReceiveAsync(TransferOrderReceiveDto receiveDto);

        /// <summary>Beklemedeki transferi iptal eder, rezervasyonları serbest bırakır.</summary>
        Task CancelAsync(TransferOrderCancelDto cancelDto);

        #endregion

        #region Read Operations

        /// <summary>Tüm transfer fişlerini listeler (RLS Korumalı).</summary>
        Task<IEnumerable<TransferOrderListDto>> GetAllAsync();

        /// <summary>Sayfalamalı transfer listesi döner (RLS Korumalı).</summary>
        Task<PagedResult<TransferOrderListDto>> GetPagedAsync(PaginationParams paginationParams);

        /// <summary>Kaynak depoya göre transferleri listeler.</summary>
        Task<IEnumerable<TransferOrderListDto>> GetAllBySourceWarehouseIdAsync(Guid warehouseId);

        /// <summary>Hedef depoya göre transferleri listeler.</summary>
        Task<IEnumerable<TransferOrderListDto>> GetAllByTargetWarehouseIdAsync(Guid warehouseId);

        /// <summary>ID ile detaylı transfer belgesini getirir.</summary>
        Task<TransferOrderDetailDto> GetByIdAsync(Guid id);

        #endregion
    }
}