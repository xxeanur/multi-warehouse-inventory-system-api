using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiWarehouse.Service.Services.Interfaces.Inventory;
using MultiWarehouse.Shared.DTOs;
using MultiWarehouse.Shared.DTOs.StockDtos;
using MultiWarehouse.Shared.Pagination;

namespace MultiWarehouse.API.Controllers
{
    /// <summary>
    /// Depo içindeki anlık stok adetlerini, rezerve durumlarını ve raf konumlarını sorgulayan Controller.
    /// </summary>
    [Authorize(Roles = "SuperAdmin,WarehouseManager,Staff")] // Staff personeli stoğu görebilmelidir
    [Route("api/[controller]")]
    [ApiController]
    public class StocksController : ControllerBase
    {
        private readonly IStockService _stockService;

        public StocksController(IStockService stockService)
        {
            _stockService = stockService;
        }

        #region Read Operations (Non-Paged)

        /// <summary>
        /// Belirtilen ID'ye sahip anlık stok kaydının detaylarını getirir.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var stock = await _stockService.GetByIdAsync(id);
            return Ok(CustomResponseDto<StockDto>.SuccessResponse(stock));
        }

        /// <summary>
        /// Sistemdeki miktarı 0'dan büyük olan tüm aktif stokları listeler.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var stocks = await _stockService.GetAllAsync();
            return Ok(CustomResponseDto<IEnumerable<StockDto>>.SuccessResponse(stocks));
        }

        /// <summary>
        /// Belirli bir ürüne ait tüm depolardaki/raflardaki stok durumunu listeler.
        /// </summary>
        [HttpGet("GetByProductId/{productId}")]
        public async Task<IActionResult> GetByProductId(Guid productId)
        {
            var stocks = await _stockService.GetAllByProductIdAsync(productId);
            return Ok(CustomResponseDto<IEnumerable<StockDto>>.SuccessResponse(stocks));
        }

        /// <summary>
        /// Sadece belirli bir deponun içindeki tüm anlık stokları listeler.
        /// </summary>
        [HttpGet("GetByWarehouseId/{warehouseId}")]
        public async Task<IActionResult> GetByWarehouseId(Guid warehouseId)
        {
            var stocks = await _stockService.GetAllByWarehouseIdAsync(warehouseId);
            return Ok(CustomResponseDto<IEnumerable<StockDto>>.SuccessResponse(stocks));
        }

        /// <summary>
        /// Sadece belirli bir rafın içindeki anlık stok durumunu listeler.
        /// </summary>
        [HttpGet("GetByShelfId/{shelfId}")]
        public async Task<IActionResult> GetByShelfId(Guid shelfId)
        {
            var stocks = await _stockService.GetAllByShelfIdAsync(shelfId);
            return Ok(CustomResponseDto<IEnumerable<StockDto>>.SuccessResponse(stocks));
        }

        #endregion

        #region Read Operations (Paged)

        /// <summary>
        /// Sistemdeki stokları sayfalama (Pagination) destekli olarak getirir.
        /// </summary>
        [HttpGet("Paged")]
        public async Task<IActionResult> GetPaged([FromQuery] PaginationParams paginationParams)
        {
            var pagedStocks = await _stockService.GetPagedAsync(paginationParams);
            return Ok(CustomResponseDto<PagedResult<StockDto>>.SuccessResponse(pagedStocks));
        }

        /// <summary>
        /// Belirli bir ürüne ait stokları sayfalama (Pagination) destekli olarak getirir.
        /// </summary>
        [HttpGet("PagedByProduct/{productId}")]
        public async Task<IActionResult> GetPagedByProduct([FromQuery] PaginationParams paginationParams, Guid productId)
        {
            var pagedStocks = await _stockService.GetPagedByProductIdAsync(paginationParams, productId);
            return Ok(CustomResponseDto<PagedResult<StockDto>>.SuccessResponse(pagedStocks));
        }

        /// <summary>
        /// Sadece belirli bir depoya ait stokları sayfalama (Pagination) destekli olarak getirir.
        /// </summary>
        [HttpGet("PagedByWarehouse/{warehouseId}")]
        public async Task<IActionResult> GetPagedByWarehouse([FromQuery] PaginationParams paginationParams, Guid warehouseId)
        {
            var pagedStocks = await _stockService.GetPagedByWarehouseIdAsync(paginationParams, warehouseId);
            return Ok(CustomResponseDto<PagedResult<StockDto>>.SuccessResponse(pagedStocks));
        }

        /// <summary>
        /// Sadece belirli bir rafa ait stokları sayfalama (Pagination) destekli olarak getirir.
        /// </summary>
        [HttpGet("PagedByShelf/{shelfId}")]
        public async Task<IActionResult> GetPagedByShelf([FromQuery] PaginationParams paginationParams, Guid shelfId)
        {
            var pagedStocks = await _stockService.GetPagedByShelfIdAsync(paginationParams, shelfId);
            return Ok(CustomResponseDto<PagedResult<StockDto>>.SuccessResponse(pagedStocks));
        }

        #endregion
    }
}