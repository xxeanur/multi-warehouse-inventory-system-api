using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiWarehouse.Service.Services.Interfaces.Documents;
using MultiWarehouse.Shared.DTOs;
using MultiWarehouse.Shared.DTOs.DocumentDtos.TransferDtos;
using MultiWarehouse.Shared.Pagination;

namespace MultiWarehouse.API.Controllers
{
    /// <summary>
    /// Depolar arası Transfer işlemlerini (Create, Dispatch, Receive, Cancel) yöneten API.
    /// Depo sınırları (RLS) servis katmanında güvence altına alınmıştır.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin,WarehouseManager,Staff")]
    public class TransferOrdersController : ControllerBase
    {
        private readonly ITransferOrderService _transferOrderService;

        public TransferOrdersController(ITransferOrderService transferOrderService)
        {
            _transferOrderService = transferOrderService;
        }

        #region Write Operations

        /// <summary>
        /// Yeni bir Transfer fişi oluşturur (Pending). Kaynak depodaki stokları rezerve eder.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,WarehouseManager")]
        public async Task<IActionResult> Create(TransferOrderCreateDto createDto)
        {
            var orderId = await _transferOrderService.CreateAsync(createDto);
            return Ok(CustomResponseDto<Guid>.SuccessResponse(orderId));
        }

        /// <summary>
        /// Beklemedeki transfer fişini yola çıkarır (Dispatch). Kaynak depodan stoklar düşülür, belge InTransit olur.
        /// </summary>
        [HttpPost("Dispatch")]
        public async Task<IActionResult> Dispatch(TransferOrderDispatchDto dispatchDto)
        {
            await _transferOrderService.DispatchAsync(dispatchDto);
            return Ok(CustomResponseDto.SuccessResponse());
        }

        /// <summary>
        /// Yoldaki (InTransit) transfer fişini hedef depoya teslim alır (Receive). Hedef depoda stoklar artar, belge Completed olur.
        /// </summary>
        [HttpPost("Receive")]
        public async Task<IActionResult> Receive(TransferOrderReceiveDto receiveDto)
        {
            await _transferOrderService.ReceiveAsync(receiveDto);
            return Ok(CustomResponseDto.SuccessResponse());
        }

        /// <summary>
        /// Henüz yola çıkmamış (Pending) transfer fişini iptal eder. Rezerve edilen kaynak stokları serbest bırakır.
        /// </summary>
        [HttpPost("Cancel")]
        [Authorize(Roles = "SuperAdmin,WarehouseManager")]
        public async Task<IActionResult> Cancel(TransferOrderCancelDto cancelDto)
        {
            await _transferOrderService.CancelAsync(cancelDto);
            return Ok(CustomResponseDto.SuccessResponse());
        }

        #endregion

        #region Read Operations

        /// <summary>
        /// Sistemdeki tüm Transfer fişlerini listeler.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _transferOrderService.GetAllAsync();
            return Ok(CustomResponseDto<IEnumerable<TransferOrderListDto>>.SuccessResponse(orders));
        }

        /// <summary>
        /// Transfer fişlerini sayfalayarak (Pagination) listeler.
        /// </summary>
        [HttpGet("Paged")]
        public async Task<IActionResult> GetPaged([FromQuery] PaginationParams paginationParams)
        {
            var pagedOrders = await _transferOrderService.GetPagedAsync(paginationParams);
            return Ok(CustomResponseDto<PagedResult<TransferOrderListDto>>.SuccessResponse(pagedOrders));
        }

        /// <summary>
        /// Sadece belirtilen KAYNAK depoya ait Transfer fişlerini listeler.
        /// </summary>
        [HttpGet("SourceWarehouse/{warehouseId}")]
        public async Task<IActionResult> GetBySourceWarehouse(Guid warehouseId)
        {
            var orders = await _transferOrderService.GetAllBySourceWarehouseIdAsync(warehouseId);
            return Ok(CustomResponseDto<IEnumerable<TransferOrderListDto>>.SuccessResponse(orders));
        }

        /// <summary>
        /// Sadece belirtilen HEDEF depoya ait Transfer fişlerini listeler.
        /// </summary>
        [HttpGet("TargetWarehouse/{warehouseId}")]
        public async Task<IActionResult> GetByTargetWarehouse(Guid warehouseId)
        {
            var orders = await _transferOrderService.GetAllByTargetWarehouseIdAsync(warehouseId);
            return Ok(CustomResponseDto<IEnumerable<TransferOrderListDto>>.SuccessResponse(orders));
        }

        /// <summary>
        /// Belirtilen ID'ye sahip Transfer fişinin satır, raf ve ürün detaylarını getirir.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var orderDetail = await _transferOrderService.GetByIdAsync(id);
            return Ok(CustomResponseDto<TransferOrderDetailDto>.SuccessResponse(orderDetail));
        }

        #endregion
    }
}