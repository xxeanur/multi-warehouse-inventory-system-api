using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiWarehouse.Service.Services.Interfaces.Inventory;
using MultiWarehouse.Shared.DTOs;
using MultiWarehouse.Shared.DTOs.ShelfDtos;
using MultiWarehouse.Shared.Pagination;

namespace MultiWarehouse.API.Controllers
{
    [Authorize] // Okuma işlemleri Staff, Manager ve SuperAdmin'e açıktır.
    [Route("api/[controller]")]
    [ApiController]
    public class ShelvesController : ControllerBase
    {
        private readonly IShelfService _shelfService;

        public ShelvesController(IShelfService shelfService)
        {
            _shelfService = shelfService;
        }

        #region Write Operations (SuperAdmin & WarehouseManager Only)

        /// <summary>
        /// Depo bloğuna yeni bir raf oluşturur. (SuperAdmin veya Kendi Deposundaki Manager)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,WarehouseManager")]
        public async Task<IActionResult> Create(ShelfCreateDto createDto)
        {
            var shelf = await _shelfService.CreateAsync(createDto);
            return Ok(CustomResponseDto<ShelfDto>.SuccessResponse(shelf));
        }

        /// <summary>
        /// Mevcut bir rafın fiziksel sınırlarını ve durumunu günceller. (SuperAdmin veya Kendi Deposundaki Manager)
        /// </summary>
        [HttpPut]
        [Authorize(Roles = "SuperAdmin,WarehouseManager")]
        public async Task<IActionResult> Update(ShelfUpdateDto updateDto)
        {
            var shelf = await _shelfService.UpdateAsync(updateDto);
            return Ok(CustomResponseDto<ShelfDto>.SuccessResponse(shelf));
        }

        /// <summary>
        /// Belirtilen rafı sistemden siler (pasife çeker). İçi dolu raflar silinemez. (SuperAdmin veya Kendi Deposundaki Manager)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,WarehouseManager")]
        public async Task<IActionResult> Remove(Guid id)
        {
            await _shelfService.RemoveAsync(id);
            return Ok(CustomResponseDto.SuccessResponse());
        }

        #endregion

        #region Read Operations (All Authenticated Users)

        /// <summary>
        /// Tüm aktif rafları listeler. (Kullanıcı yetkisine göre filtrelenir)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var shelves = await _shelfService.GetAllAsync();
            return Ok(CustomResponseDto<IEnumerable<ShelfDto>>.SuccessResponse(shelves));
        }

        /// <summary>
        /// Belirtilen ID'ye sahip rafı getirir.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var shelf = await _shelfService.GetByIdAsync(id);
            return Ok(CustomResponseDto<ShelfDto>.SuccessResponse(shelf));
        }

        /// <summary>
        /// Belirli bir bloğun (Zone) içindeki tüm rafları listeler.
        /// </summary>
        [HttpGet("GetByZoneId/{zoneId}")]
        public async Task<IActionResult> GetByZoneId(Guid zoneId)
        {
            var shelves = await _shelfService.GetAllByZoneIdAsync(zoneId);
            return Ok(CustomResponseDto<IEnumerable<ShelfDto>>.SuccessResponse(shelves));
        }

        /// <summary>
        /// Sistemdeki tüm rafları pagination olarak getirir.
        /// Örnek: GET /api/Shelves/Paged?pageNumber=1&pageSize=20
        /// </summary>
        [HttpGet("Paged")]
        public async Task<IActionResult> GetPaged([FromQuery] PaginationParams paginationParams)
        {
            var pagedShelves = await _shelfService.GetPagedAsync(paginationParams);
            return Ok(CustomResponseDto<PagedResult<ShelfDto>>.SuccessResponse(pagedShelves));
        }

        /// <summary>
        /// Sadece belirtilen Zone ID'sine ait rafları sayfalayarak getirir.
        /// Örnek: GET /api/Shelves/PagedByZone/12345-abcde...?pageNumber=1&pageSize=20
        /// </summary>
        [HttpGet("PagedByZone/{zoneId}")]
        public async Task<IActionResult> GetPagedByZone([FromQuery] PaginationParams paginationParams, Guid zoneId)
        {
            var pagedShelves = await _shelfService.GetPagedByZoneIdAsync(paginationParams, zoneId);
            return Ok(CustomResponseDto<PagedResult<ShelfDto>>.SuccessResponse(pagedShelves));
        }

        #endregion
    }
}
