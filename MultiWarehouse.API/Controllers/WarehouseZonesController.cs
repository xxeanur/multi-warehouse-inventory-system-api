using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiWarehouse.Service.Services.Interfaces;
using MultiWarehouse.Shared.DTOs;
using MultiWarehouse.Shared.DTOs.WarehouseZoneDtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiWarehouse.API.Controllers
{
    [Authorize(Roles = "SuperAdmin,WarehouseManager")]
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseZonesController : ControllerBase
    {
        private readonly IWarehouseZoneService _zoneService;

        public WarehouseZonesController(IWarehouseZoneService zoneService)
        {
            _zoneService = zoneService;
        }

        /// <summary>
        /// Depo içine yeni bir blok/alan ekler.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(WarehouseZoneCreateDto createDto)
        {
            var zone = await _zoneService.CreateAsync(createDto);
            return Ok(CustomResponseDto<WarehouseZoneDto>.SuccessResponse(zone));
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
        /// Tüm depolardaki tüm alanları/blokları listeler.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var zones = await _zoneService.GetAllAsync();
            return Ok(CustomResponseDto<IEnumerable<WarehouseZoneDto>>.SuccessResponse(zones));
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

        /// <summary>
        /// Mevcut bir depo alanını günceller.
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> Update(WarehouseZoneUpdateDto updateDto)
        {
            var zone = await _zoneService.UpdateAsync(updateDto);
            return Ok(CustomResponseDto<WarehouseZoneDto>.SuccessResponse(zone));
        }

        /// <summary>
        /// Belirtilen depo alanını sistemden siler (pasife çeker).
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Remove(Guid id)
        {
            await _zoneService.RemoveAsync(id);
            return Ok(CustomResponseDto.SuccessResponse());
        }
    }
}