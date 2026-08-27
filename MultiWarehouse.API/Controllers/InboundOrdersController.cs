using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiWarehouse.Service.Services.Interfaces.Documents;
using MultiWarehouse.Shared.DTOs;
using MultiWarehouse.Shared.DTOs.DocumentDtos.InboundDtos;
using MultiWarehouse.Shared.Pagination;

namespace MultiWarehouse.API.Controllers
{
    /// <summary>
    /// Mal Kabul (Inbound) operasyonlarını yöneten API.
    /// Belge yaşam döngüsü (Oluşturma, Kapıda Onay, İptal) buradan yönetilir.
    /// Depo sınırları  servis katmanında güvence altına alınmıştır.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin,WarehouseManager,Staff")] // Saha personeli de mal kabul edebilir
    public class InboundOrdersController : ControllerBase
    {
        private readonly IInboundOrderService _inboundOrderService;

        public InboundOrdersController(IInboundOrderService inboundOrderService)
        {
            _inboundOrderService = inboundOrderService;
        }

        #region Write Operations

        /// <summary>
        /// Yeni bir Mal Kabul (Inbound) fişi oluşturur. Durumu "Pending" olarak atanır.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(InboundOrderCreateDto createDto)
        {
            var orderId = await _inboundOrderService.CreateAsync(createDto);
            return Ok(CustomResponseDto<Guid>.SuccessResponse(orderId));
        }

        /// <summary>
        /// Beklemede olan bir fişin mallarını kapıda sayar ve onaylar (Approved).
        /// Raflama için Putaway servisi beklenir.
        /// </summary>
        [HttpPost("Approve")]
        public async Task<IActionResult> Approve(InboundOrderApproveDto approveDto)
        {
            await _inboundOrderService.ApproveAsync(approveDto);
            return Ok(CustomResponseDto.SuccessResponse());
        }

        /// <summary>
        /// Tamamlanmamış (Completed olmayan) bir Mal Kabul fişini iptal eder.
        /// </summary>
        [HttpPost("{id}/Cancel")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            await _inboundOrderService.CancelAsync(id);
            return Ok(CustomResponseDto.SuccessResponse());
        }

        #endregion

        #region Read Operations

        /// <summary>
        /// Sistemdeki tüm Mal Kabul fişlerini listeler.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _inboundOrderService.GetAllAsync();
            return Ok(CustomResponseDto<IEnumerable<InboundOrderListDto>>.SuccessResponse(orders));
        }

        /// <summary>
        /// Mal Kabul fişlerini sayfalayarak (Pagination) listeler.
        /// </summary>
        [HttpGet("Paged")]
        public async Task<IActionResult> GetPaged([FromQuery] PaginationParams paginationParams)
        {
            var pagedOrders = await _inboundOrderService.GetPagedAsync(paginationParams);
            return Ok(CustomResponseDto<PagedResult<InboundOrderListDto>>.SuccessResponse(pagedOrders));
        }

        /// <summary>
        /// Sadece belirtilen depoya ait Mal Kabul fişlerini listeler.
        /// </summary>
        [HttpGet("Warehouse/{warehouseId}")]
        public async Task<IActionResult> GetByWarehouse(Guid warehouseId)
        {
            var orders = await _inboundOrderService.GetAllByWarehouseIdAsync(warehouseId);
            return Ok(CustomResponseDto<IEnumerable<InboundOrderListDto>>.SuccessResponse(orders));
        }

        /// <summary>
        /// Belirtilen ID'ye sahip Mal Kabul fişinin, satır ve ürün detaylarını getirir.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var orderDetail = await _inboundOrderService.GetByIdAsync(id);
            return Ok(CustomResponseDto<InboundOrderDetailDto>.SuccessResponse(orderDetail));
        }

        #endregion
    }
}