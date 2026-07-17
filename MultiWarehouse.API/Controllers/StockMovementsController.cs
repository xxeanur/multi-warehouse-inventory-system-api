using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiWarehouse.Service.Services.Interfaces;
using MultiWarehouse.Shared.DTOs;
using MultiWarehouse.Shared.DTOs.StockMovementDtos;
using MultiWarehouse.Shared.Pagination;

namespace MultiWarehouse.API.Controllers
{
    /// <summary>
    /// Depo içerisindeki veya dışarısındaki tüm mal hareketlerinin kayıt altına alındığı API.
    /// </summary>
    [Authorize(Roles = "SuperAdmin,WarehouseManager")]
    [Route("api/[controller]")]
    [ApiController]
    public class StockMovementsController : ControllerBase
    {
        private readonly IStockMovementService _movementService;

        public StockMovementsController(IStockMovementService movementService)
        {
            _movementService = movementService;
        }

        /// <summary>Yeni bir stok hareketi (Giriş, Çıkış, Transfer) kaydeder.</summary>
        [HttpPost]
        public async Task<IActionResult> Create(StockMovementCreateDto createDto)
        {
            var movement = await _movementService.CreateAsync(createDto);
            return Ok(CustomResponseDto<StockMovementDto>.SuccessResponse(movement));
        }

        /// <summary>Belirtilen ID'ye sahip hareket detayını getirir.</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var movement = await _movementService.GetByIdAsync(id);
            return Ok(CustomResponseDto<StockMovementDto>.SuccessResponse(movement));
        }

        /// <summary>Sistemdeki tüm stok hareket geçmişini (Ledger) getirir.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var movements = await _movementService.GetAllAsync();
            return Ok(CustomResponseDto<IEnumerable<StockMovementDto>>.SuccessResponse(movements));
        }

        /// <summary>Spesifik bir ürüne ait tüm hareketleri listeler.</summary>
        [HttpGet("GetByProductId/{productId}")]
        public async Task<IActionResult> GetByProductId(Guid productId)
        {
            var movements = await _movementService.GetAllByProductIdAsync(productId);
            return Ok(CustomResponseDto<IEnumerable<StockMovementDto>>.SuccessResponse(movements));
        }

        /// <summary>Spesifik bir depoya giren veya çıkan tüm hareketleri listeler.</summary>
        [HttpGet("GetByWarehouseId/{warehouseId}")]
        public async Task<IActionResult> GetByWarehouseId(Guid warehouseId)
        {
            var movements = await _movementService.GetAllByWarehouseIdAsync(warehouseId);
            return Ok(CustomResponseDto<IEnumerable<StockMovementDto>>.SuccessResponse(movements));
        }
        //pagination
        /// <summary>
        /// Sistemdeki tüm stok hareketlerini (Tarihçe) sayfalama destekli olarak getirir.
        /// </summary>
        [HttpGet("Paged")]
        public async Task<IActionResult> GetPaged([FromQuery] PaginationParams paginationParams)
        {
            var pagedMovements = await _movementService.GetPagedAsync(paginationParams);
            return Ok(CustomResponseDto<PagedResult<StockMovementDto>>.SuccessResponse(pagedMovements));
        }

        /// <summary>
        /// Sadece belirtilen ürüne ait hareket tarihçesini sayfalayarak getirir.
        /// </summary>
        [HttpGet("PagedByProduct/{productId}")]
        public async Task<IActionResult> GetPagedByProduct([FromQuery] PaginationParams paginationParams, Guid productId)
        {
            var pagedMovements = await _movementService.GetPagedByProductIdAsync(paginationParams, productId);
            return Ok(CustomResponseDto<PagedResult<StockMovementDto>>.SuccessResponse(pagedMovements));
        }

        /// <summary>
        /// Sadece belirtilen depoda gerçekleşen (giriş/çıkış) hareketleri sayfalayarak getirir.
        /// </summary>
        [HttpGet("PagedByWarehouse/{warehouseId}")]
        public async Task<IActionResult> GetPagedByWarehouse([FromQuery] PaginationParams paginationParams, Guid warehouseId)
        {
            var pagedMovements = await _movementService.GetPagedByWarehouseIdAsync(paginationParams, warehouseId);
            return Ok(CustomResponseDto<PagedResult<StockMovementDto>>.SuccessResponse(pagedMovements));
        }

        /// <summary>Mevcut bir hareketin durumunu, açıklamasını veya referans numarasını günceller. (Miktar güncellenemez!)</summary>
        [HttpPut]
        public async Task<IActionResult> Update(StockMovementUpdateDto updateDto)
        {
            var movement = await _movementService.UpdateAsync(updateDto);
            return Ok(CustomResponseDto<StockMovementDto>.SuccessResponse(movement));
        }

        /// <summary>Stok hareketini sistemden pasife çeker.</summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Remove(Guid id)
        {
            await _movementService.RemoveAsync(id);
            return Ok(CustomResponseDto.SuccessResponse());
        }
    }
}