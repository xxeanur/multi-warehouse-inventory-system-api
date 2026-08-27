using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiWarehouse.Service.Services.Interfaces.Inventory;
using MultiWarehouse.Shared.DTOs;
using MultiWarehouse.Shared.DTOs.CountDtos;

namespace MultiWarehouse.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Güvenlik: Sadece operasyonel yetkiye sahip roller sayım yapabilir.
    [Authorize(Roles = "SuperAdmin,WarehouseManager,Staff")]
    public class InventoryCountsController : ControllerBase
    {
        private readonly IInventoryCountService _inventoryCountService;

        public InventoryCountsController(IInventoryCountService inventoryCountService)
        {
            _inventoryCountService = inventoryCountService;
        }

        #region Command Operations

        /// <summary>
        /// Operatörün girdiği fiziki sayım sonucunu işler. 
        /// Stok miktarlarını, depo/raf kapasitelerini günceller ve sistem denetim logu oluşturur.
        /// </summary>
        [HttpPost("PerformCount")]
        public async Task<IActionResult> PerformCount([FromBody] InventoryCountCreateDto countDto)
        {
            // UserId okuma ve Depo RLS (Row-Level Security) doğrulama işlemleri Servis katmanında ele alınır.
            var result = await _inventoryCountService.PerformCountAsync(countDto);
            return Ok(CustomResponseDto<InventoryCountResultDto>.SuccessResponse(result));
        }

        #endregion
    }
}