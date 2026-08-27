using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiWarehouse.Service.Services.Interfaces.Definations;
using MultiWarehouse.Shared.DTOs;
using MultiWarehouse.Shared.DTOs.WarehouseDtos;
using MultiWarehouse.Shared.Pagination;

namespace MultiWarehouse.API.Controllers
{
    [Authorize] // Okuma işlemleri (Get) tüm giriş yapmış kullanıcılara (Manager, Staff) açıktır.
    [Route("api/[controller]")]
    [ApiController]
    public class WarehousesController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;

        public WarehousesController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        #region Write Operations (Only SuperAdmin)

        /// <summary>
        /// Sisteme yeni bir depo ekler. Sadece SuperAdmin yetkilidir.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Create(WarehouseCreateDto createDto)
        {
            var warehouse = await _warehouseService.CreateAsync(createDto);
            return Ok(CustomResponseDto<WarehouseDto>.SuccessResponse(warehouse));
        }

        /// <summary>
        /// Mevcut bir deponun bilgilerini günceller. Sadece SuperAdmin yetkilidir.
        /// </summary>
        [HttpPut]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Update(WarehouseUpdateDto updateDto)
        {
            var warehouse = await _warehouseService.UpdateAsync(updateDto);
            return Ok(CustomResponseDto<WarehouseDto>.SuccessResponse(warehouse));
        }

        /// <summary>
        /// Belirtilen depoyu sistemden siler (pasife çeker). İçi dolu depolar silinemez. Sadece SuperAdmin yetkilidir.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Remove(Guid id)
        {
            await _warehouseService.RemoveAsync(id);
            return Ok(CustomResponseDto.SuccessResponse());
        }

        #endregion

        #region Read Operations (All Authenticated Users)

        /// <summary>
        /// Sistemdeki tüm aktif depoları listeler.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var warehouses = await _warehouseService.GetAllAsync();
            return Ok(CustomResponseDto<IEnumerable<WarehouseDto>>.SuccessResponse(warehouses));
        }

        /// <summary>
        /// Sistemdeki depoları sayfalama (Pagination) destekli olarak getirir.
        /// Örnek Kullanım: GET /api/Warehouses/Paged?pageNumber=1&pageSize=10
        /// </summary>
        [HttpGet("Paged")]
        public async Task<IActionResult> GetPaged([FromQuery] PaginationParams paginationParams)
        {
            var pagedWarehouses = await _warehouseService.GetPagedAsync(paginationParams);
            return Ok(CustomResponseDto<PagedResult<WarehouseDto>>.SuccessResponse(pagedWarehouses));
        }

        /// <summary>
        /// Belirtilen ID'ye sahip depoyu detaylarıyla getirir.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var warehouse = await _warehouseService.GetByIdAsync(id);
            return Ok(CustomResponseDto<WarehouseDto>.SuccessResponse(warehouse));
        }

        #endregion
    }
}