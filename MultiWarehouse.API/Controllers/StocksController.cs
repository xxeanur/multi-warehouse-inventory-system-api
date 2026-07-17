// MultiWarehouse.API/Controllers/StocksController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiWarehouse.Service.Services.Interfaces;
using MultiWarehouse.Shared.DTOs;
using MultiWarehouse.Shared.DTOs.StockDtos;
using MultiWarehouse.Shared.Pagination;

namespace MultiWarehouse.API.Controllers
{
    /// <summary>
    /// Depo içindeki anlık stok adetlerini, rezerve durumlarını ve raf konumlarını yöneten Controller.
    /// </summary>
    [Authorize(Roles = "SuperAdmin,WarehouseManager")]
    [Route("api/[controller]")]
    [ApiController]
    public class StocksController : ControllerBase
    {
        private readonly IStockService _stockService;

        public StocksController(IStockService stockService)
        {
            _stockService = stockService;
        }

        /// <summary>
        /// Sisteme yeni bir stok satırı ekler.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(StockCreateDto createDto)
        {
            var stock = await _stockService.CreateAsync(createDto);
            return Ok(CustomResponseDto<StockDto>.SuccessResponse(stock));
        }

        /// <summary>
        /// Belirtilen ID'ye sahip stok kaydını getirir.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var stock = await _stockService.GetByIdAsync(id);
            return Ok(CustomResponseDto<StockDto>.SuccessResponse(stock));
        }

        /// <summary>
        /// Tüm sistemdeki aktif stok kayıtlarını listeler.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var stocks = await _stockService.GetAllAsync();
            return Ok(CustomResponseDto<IEnumerable<StockDto>>.SuccessResponse(stocks));
        }

        /// <summary>
        /// Bir ürünün tüm depolardaki ve raflardaki dağılımını (stoklarını) listeler.
        /// </summary>
        [HttpGet("GetByProductId/{productId}")]
        public async Task<IActionResult> GetByProductId(Guid productId)
        {
            var stocks = await _stockService.GetAllByProductIdAsync(productId);
            return Ok(CustomResponseDto<IEnumerable<StockDto>>.SuccessResponse(stocks));
        }

        /// <summary>
        /// Belirli bir depodaki tüm stokları listeler.
        /// </summary>
        [HttpGet("GetByWarehouseId/{warehouseId}")]
        public async Task<IActionResult> GetByWarehouseId(Guid warehouseId)
        {
            var stocks = await _stockService.GetAllByWarehouseIdAsync(warehouseId);
            return Ok(CustomResponseDto<IEnumerable<StockDto>>.SuccessResponse(stocks));
        }

        /// <summary>
        /// Sadece tek bir rafta bulunan ürünlerin stok durumunu listeler.
        /// </summary>
        [HttpGet("GetByShelfId/{shelfId}")]
        public async Task<IActionResult> GetByShelfId(Guid shelfId)
        {
            var stocks = await _stockService.GetAllByShelfIdAsync(shelfId);
            return Ok(CustomResponseDto<IEnumerable<StockDto>>.SuccessResponse(stocks));
        }

        /// <summary>
        /// Sistemdeki tüm stokları sayfalama (Pagination) destekli olarak getirir.
        /// </summary>
        [HttpGet("Paged")]
        public async Task<IActionResult> GetPaged([FromQuery] PaginationParams paginationParams)
        {
            var pagedStocks = await _stockService.GetPagedAsync(paginationParams);
            return Ok(CustomResponseDto<PagedResult<StockDto>>.SuccessResponse(pagedStocks));
        }

        /// <summary>
        /// Sadece belirtilen ürüne ait stokları sayfalayarak getirir.
        /// </summary>
        [HttpGet("PagedByProduct/{productId}")]
        public async Task<IActionResult> GetPagedByProduct([FromQuery] PaginationParams paginationParams, Guid productId)
        {
            var pagedStocks = await _stockService.GetPagedByProductIdAsync(paginationParams, productId);
            return Ok(CustomResponseDto<PagedResult<StockDto>>.SuccessResponse(pagedStocks));
        }

        /// <summary>
        /// Sadece belirtilen depoya ait stokları sayfalayarak getirir.
        /// </summary>
        [HttpGet("PagedByWarehouse/{warehouseId}")]
        public async Task<IActionResult> GetPagedByWarehouse([FromQuery] PaginationParams paginationParams, Guid warehouseId)
        {
            var pagedStocks = await _stockService.GetPagedByWarehouseIdAsync(paginationParams, warehouseId);
            return Ok(CustomResponseDto<PagedResult<StockDto>>.SuccessResponse(pagedStocks));
        }

        /// <summary>
        /// Sadece belirtilen rafa ait stokları sayfalayarak getirir.
        /// </summary>
        [HttpGet("PagedByShelf/{shelfId}")]
        public async Task<IActionResult> GetPagedByShelf([FromQuery] PaginationParams paginationParams, Guid shelfId)
        {
            var pagedStocks = await _stockService.GetPagedByShelfIdAsync(paginationParams, shelfId);
            return Ok(CustomResponseDto<PagedResult<StockDto>>.SuccessResponse(pagedStocks));
        }

        /// <summary>
        /// Mevcut bir stok kaydının miktarını, konumunu veya rezerve durumunu günceller.
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> Update(StockUpdateDto updateDto)
        {
            var stock = await _stockService.UpdateAsync(updateDto);
            return Ok(CustomResponseDto<StockDto>.SuccessResponse(stock));
        }

        /// <summary>
        /// Stok kaydını pasife çeker. (Miktarı 0'dan büyük olan stoklar silinemez).
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Remove(Guid id)
        {
            await _stockService.RemoveAsync(id);
            return Ok(CustomResponseDto.SuccessResponse());
        }
    }
}