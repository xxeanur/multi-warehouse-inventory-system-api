// MultiWarehouse.API/Controllers/WarehousesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiWarehouse.Service.Services.Interfaces;
using MultiWarehouse.Shared.DTOs;
using MultiWarehouse.Shared.DTOs.WarehouseDtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiWarehouse.API.Controllers
{
    [Authorize(Roles = "SuperAdmin,WarehouseManager")]
    [Route("api/[controller]")]
    [ApiController]
    public class WarehousesController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;

        public WarehousesController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        /// <summary>
        /// Sisteme yeni bir depo ekler.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(WarehouseCreateDto createDto)
        {
            var warehouse = await _warehouseService.CreateAsync(createDto);
            return Ok(CustomResponseDto<WarehouseDto>.SuccessResponse(warehouse));
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
        /// Mevcut bir deponun bilgilerini günceller.
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> Update(WarehouseUpdateDto updateDto)
        {
            var warehouse = await _warehouseService.UpdateAsync(updateDto);
            return Ok(CustomResponseDto<WarehouseDto>.SuccessResponse(warehouse));
        }

        /// <summary>
        /// Belirtilen depoyu sistemden siler (pasife çeker).
        /// İçi dolu depolar silinemez.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Remove(Guid id)
        {
            await _warehouseService.RemoveAsync(id);
            return Ok(CustomResponseDto.SuccessResponse());
        }
    }
}