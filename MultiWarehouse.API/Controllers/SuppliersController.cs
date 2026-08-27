using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiWarehouse.Service.Services.Interfaces.Definations;
using MultiWarehouse.Shared.DTOs;
using MultiWarehouse.Shared.DTOs.SupplierDtos;

namespace MultiWarehouse.API.Controllers
{
    [Authorize] // Okuma işlemleri tüm yetkili kullanıcılara açıktır.
    [Route("api/[controller]")]
    [ApiController]
    public class SuppliersController : ControllerBase
    {
        private readonly ISupplierService _supplierService;

        public SuppliersController(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        #region Write Operations (Only SuperAdmin)

        /// <summary>
        /// Yeni bir tedarikçi oluşturur. Sadece SuperAdmin yetkilidir.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Create(SupplierCreateDto createDto)
        {
            var supplier = await _supplierService.CreateAsync(createDto);
            return Ok(CustomResponseDto<SupplierDto>.SuccessResponse(supplier));
        }

        /// <summary>
        /// Mevcut bir tedarikçinin bilgilerini günceller. Sadece SuperAdmin yetkilidir.
        /// </summary>
        [HttpPut]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Update(SupplierUpdateDto updateDto)
        {
            var supplier = await _supplierService.UpdateAsync(updateDto);
            return Ok(CustomResponseDto<SupplierDto>.SuccessResponse(supplier));
        }

        /// <summary>
        /// Belirtilen tedarikçiyi siler (pasife çeker). Sadece SuperAdmin yetkilidir.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Remove(Guid id)
        {
            await _supplierService.RemoveAsync(id);
            return Ok(CustomResponseDto.SuccessResponse());
        }

        #endregion

        #region Read Operations (All Authenticated Users)

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
        /// Belirtilen ID'ye sahip tedarikçiyi getirir.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var supplier = await _supplierService.GetByIdAsync(id);
            return Ok(CustomResponseDto<SupplierDto>.SuccessResponse(supplier));
        }

        #endregion
    }
}