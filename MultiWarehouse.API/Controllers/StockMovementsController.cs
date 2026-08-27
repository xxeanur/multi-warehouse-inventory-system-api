using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiWarehouse.Service.Services.Interfaces.Inventory;
using MultiWarehouse.Shared.DTOs;
using MultiWarehouse.Shared.DTOs.InventoryDtos;
using MultiWarehouse.Shared.DTOs.StockMovementDtos;
using MultiWarehouse.Shared.Pagination;

namespace MultiWarehouse.API.Controllers
{
    /// <summary>
    /// Stok hareket defterini sorgulayan read-only API.s
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin,WarehouseManager")]
    public class StockMovementsController : ControllerBase
    {
        private readonly IStockMovementService _stockMovementService;

        public StockMovementsController(IStockMovementService stockMovementService)
        {
            _stockMovementService = stockMovementService;
        }

        #region Read Operations

        /// <summary>
        /// Filtrelere göre stok hareketlerini sayfalamalı getirir.
        /// </summary>
        [HttpGet("Filtered")]
        public async Task<IActionResult> GetFilteredPaged([FromQuery] StockMovementFilterDto filterDto, [FromQuery] PaginationParams paginationParams)
        {
            var result = await _stockMovementService.GetFilteredPagedAsync(filterDto, paginationParams);
            return Ok(CustomResponseDto<PagedResult<StockMovementListDto>>.SuccessResponse(result));
        }

        /// <summary>
        /// Stok hareket detayını getirir.
        /// </summary>
        [HttpGet("{id}/detail")]
        public async Task<IActionResult> GetDetailById(Guid id)
        {
            var result = await _stockMovementService.GetDetailByIdAsync(id);
            return Ok(CustomResponseDto<StockMovementDetailDto>.SuccessResponse(result));
        }

        /// <summary>
        /// Ürüne ait hareket geçmişini sayfalamalı listeler.
        /// </summary>
        [HttpGet("Product/{productId}")]
        public async Task<IActionResult> GetByProduct(Guid productId, [FromQuery] PaginationParams paginationParams)
        {
            var result = await _stockMovementService.GetByProductIdAsync(productId, paginationParams);
            return Ok(CustomResponseDto<PagedResult<StockMovementListDto>>.SuccessResponse(result));
        }

        /// <summary>
        /// Rafa yapılmış hareket geçmişini sayfalamalı listeler.
        /// </summary>
        [HttpGet("Shelf/{shelfId}")]
        public async Task<IActionResult> GetByShelf(Guid shelfId, [FromQuery] PaginationParams paginationParams)
        {
            var result = await _stockMovementService.GetByShelfIdAsync(shelfId, paginationParams);
            return Ok(CustomResponseDto<PagedResult<StockMovementListDto>>.SuccessResponse(result));
        }

        /// <summary>
        /// Belgeye bağlı hareketleri sayfalamalı listeler.
        /// </summary>
        [HttpGet("Document/{documentId}")]
        public async Task<IActionResult> GetByDocument(Guid documentId, [FromQuery] PaginationParams paginationParams)
        {
            var result = await _stockMovementService.GetByDocumentIdAsync(documentId, paginationParams);
            return Ok(CustomResponseDto<PagedResult<StockMovementListDto>>.SuccessResponse(result));
        }

        #endregion
    }
}