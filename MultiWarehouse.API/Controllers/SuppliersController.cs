using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiWarehouse.Service.Services.Interfaces;
using MultiWarehouse.Shared.DTOs;
using MultiWarehouse.Shared.DTOs.SupplierDtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiWarehouse.API.Controllers
{
    [Authorize(Roles = "SuperAdmin,WarehouseManager")]
    [Route("api/[controller]")]
    [ApiController]
    public class SuppliersController : ControllerBase
    {
        private readonly ISupplierService _supplierService;

        public SuppliersController(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        /// <summary>
        /// Yeni bir tedarikçi oluşturur.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(SupplierCreateDto createDto)
        {
            var supplier = await _supplierService.CreateAsync(createDto);
            return Ok(CustomResponseDto<SupplierDto>.SuccessResponse(supplier));
        }

        /// <summary>
        /// Belirtilen ID'ye sahip tedarikçiyi getirir.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var supplier = await _supplierService.GetByIdAsync(id);
            return Ok(CustomResponseDto<SupplierDto>.SuccessResponse(supplier));
        }

        /// <summary>
        /// Tüm aktif tedarikçileri listeler.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var suppliers = await _supplierService.GetAllAsync();
            return Ok(CustomResponseDto<IEnumerable<SupplierDto>>.SuccessResponse(suppliers));
        }

        /// <summary>
        /// Mevcut bir tedarikçinin bilgilerini günceller.
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> Update(SupplierUpdateDto updateDto)
        {
            var supplier = await _supplierService.UpdateAsync(updateDto);
            return Ok(CustomResponseDto<SupplierDto>.SuccessResponse(supplier));
        }

        /// <summary>
        /// Belirtilen tedarikçiyi siler (pasife çeker).
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Remove(Guid id)
        {
            await _supplierService.RemoveAsync(id);
            return Ok(CustomResponseDto.SuccessResponse());
        }
    }
}