using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiWarehouse.Service.Services.Interfaces.Definations;
using MultiWarehouse.Shared.DTOs;
using MultiWarehouse.Shared.DTOs.WarehouseZoneDtos;

namespace MultiWarehouse.API.Controllers
{
    [Authorize] // Okuma işlemleri tüm yetkili kullanıcılara açıktır (Staff dahil).
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseZonesController : ControllerBase
    {
        private readonly IWarehouseZoneService _zoneService;

        public WarehouseZonesController(IWarehouseZoneService zoneService)
        {
            _zoneService = zoneService;
        }

        #region Write Operations (SuperAdmin & WarehouseManager Only)

        /// <summary>
        /// Depo içine yeni bir blok/alan ekler. (SuperAdmin veya Kendi Deposundaki Manager)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,WarehouseManager")]
        public async Task<IActionResult> Create(WarehouseZoneCreateDto createDto)
        {
            var zone = await _zoneService.CreateAsync(createDto);
            return Ok(CustomResponseDto<WarehouseZoneDto>.SuccessResponse(zone));
        }

        /// <summary>
        /// Mevcut bir depo alanını günceller. (SuperAdmin veya Kendi Deposundaki Manager)
        /// </summary>
        [HttpPut]
        [Authorize(Roles = "SuperAdmin,WarehouseManager")]
        public async Task<IActionResult> Update(WarehouseZoneUpdateDto updateDto)
        {
            var zone = await _zoneService.UpdateAsync(updateDto);
            return Ok(CustomResponseDto<WarehouseZoneDto>.SuccessResponse(zone));
        }

        /// <summary>
        /// Belirtilen depo alanını sistemden siler (pasife çeker). (SuperAdmin veya Kendi Deposundaki Manager)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,WarehouseManager")]
        public async Task<IActionResult> Remove(Guid id)
        {
            await _zoneService.RemoveAsync(id);
            return Ok(CustomResponseDto.SuccessResponse());
        }

        #endregion

        #region Read Operations (All Authenticated Users)

        /// <summary>
        /// Tüm aktif alanları/blokları listeler.
        /// (Not: Sistem yetkiye göre sadece kullanıcının erişebildiği kayıtları döner).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var zones = await _zoneService.GetAllAsync();
            return Ok(CustomResponseDto<IEnumerable<WarehouseZoneDto>>.SuccessResponse(zones));
        }

        /// <summary>
        /// Belirtilen ID'ye sahip depo alanını getirir.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var zone = await _zoneService.GetByIdAsync(id);
            return Ok(CustomResponseDto<WarehouseZoneDto>.SuccessResponse(zone));
        }

        /// <summary>
        /// Belirli bir deponun içindeki alanları listeler.
        /// </summary>
        [HttpGet("GetByWarehouseId/{warehouseId}")]
        public async Task<IActionResult> GetByWarehouseId(Guid warehouseId)
        {
            var zones = await _zoneService.GetAllByWarehouseIdAsync(warehouseId);
            return Ok(CustomResponseDto<IEnumerable<WarehouseZoneDto>>.SuccessResponse(zones));
        }

        #endregion
    }
}