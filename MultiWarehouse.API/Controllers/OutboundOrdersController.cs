using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiWarehouse.Service.Services.Interfaces.Documents;
using MultiWarehouse.Shared.DTOs;
using MultiWarehouse.Shared.DTOs.DocumentDtos.OutboundDtos;
using MultiWarehouse.Shared.Pagination;

namespace MultiWarehouse.API.Controllers
{
    /// <summary>
    /// Mal Çıkış / Sevkiyat (Outbound) işlemlerini yürüten API.
    /// Belge oluşturma, ürün toplama (Picking) ve iptal yönetimi buradan sağlanır.
    /// Depo sınırları  servis katmanında güvence altına alınmıştır.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin,WarehouseManager,Staff")] // Genel okuma ve toplama için Staff yetkilidir
    public class OutboundOrdersController : ControllerBase
    {
        private readonly IOutboundOrderService _outboundOrderService;

        public OutboundOrdersController(IOutboundOrderService outboundOrderService)
        {
            _outboundOrderService = outboundOrderService;
        }

        #region Write Operations

        /// <summary>
        /// Yeni bir Mal Çıkış (Outbound) fişi oluşturur ve stokları hedef raflardan rezerve eder.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,WarehouseManager")] // Sadece Yöneticiler fiş oluşturabilir
        public async Task<IActionResult> Create(OutboundOrderCreateDto createDto)
        {
            var orderId = await _outboundOrderService.CreateAsync(createDto);
            return Ok(CustomResponseDto<Guid>.SuccessResponse(orderId));
        }

        /// <summary>
        /// Beklemede olan bir fişi, personelin sistemin tahsis ettiği raflardan topladığı miktarlarla onaylar (Completed). 
        /// </summary>
        [HttpPost("Approve")]
        public async Task<IActionResult> Approve(OutboundOrderApproveDto approveDto)
        {
            await _outboundOrderService.ApproveAsync(approveDto);
            return Ok(CustomResponseDto.SuccessResponse());
        }

        /// <summary>
        /// Henüz toplanmamış (Pending) bir Mal Çıkış fişini iptal eder. Nokta atışı rezerve edilen stoklar serbest bırakılır.
        /// </summary>
        [HttpPost("Cancel")]
        [Authorize(Roles = "SuperAdmin,WarehouseManager")] // Sadece Yöneticiler iptal edebilir
        public async Task<IActionResult> Cancel(OutboundOrderCancelDto cancelDto)
        {
            await _outboundOrderService.CancelAsync(cancelDto);
            return Ok(CustomResponseDto.SuccessResponse());
        }

        #endregion

        #region Read Operations

        /// <summary>
        /// Sistemdeki tüm Mal Çıkış fişlerini listeler.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _outboundOrderService.GetAllAsync();
            return Ok(CustomResponseDto<IEnumerable<OutboundOrderListDto>>.SuccessResponse(orders));
        }

        /// <summary>
        /// Mal Çıkış fişlerini sayfalayarak (Pagination) listeler.
        /// </summary>
        [HttpGet("Paged")]
        public async Task<IActionResult> GetPaged([FromQuery] PaginationParams paginationParams)
        {
            var pagedOrders = await _outboundOrderService.GetPagedAsync(paginationParams);
            return Ok(CustomResponseDto<PagedResult<OutboundOrderListDto>>.SuccessResponse(pagedOrders));
        }

        /// <summary>
        /// Sadece belirtilen depoya ait Mal Çıkış fişlerini listeler.
        /// </summary>
        [HttpGet("Warehouse/{warehouseId}")]
        public async Task<IActionResult> GetByWarehouse(Guid warehouseId)
        {
            var orders = await _outboundOrderService.GetAllByWarehouseIdAsync(warehouseId);
            return Ok(CustomResponseDto<IEnumerable<OutboundOrderListDto>>.SuccessResponse(orders));
        }

        /// <summary>
        /// Belirtilen ID'ye sahip Mal Çıkış fişinin satır, toplanan raf ve ürün detaylarını getirir.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var orderDetail = await _outboundOrderService.GetByIdAsync(id);
            return Ok(CustomResponseDto<OutboundOrderDetailDto>.SuccessResponse(orderDetail));
        }

        #endregion
    }
}