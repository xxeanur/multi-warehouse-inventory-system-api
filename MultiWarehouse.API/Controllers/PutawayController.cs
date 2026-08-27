using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiWarehouse.Service.Services.Interfaces.Inventory;
using MultiWarehouse.Shared.DTOs;
using MultiWarehouse.Shared.DTOs.PutawayDtos;

namespace MultiWarehouse.API.Controllers
{
    /// <summary>
    /// Depoya girişi onaylanan (Mal Kabul veya Transfer) ürünlerin, sistemin önerdiği veya personelin seçtiği raflara yerleştirilmesi (Putaway / Raflama) süreçlerini yöneten API kontrolcüsü.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin,WarehouseManager,Staff")]
    public class PutawayController : ControllerBase
    {
        private readonly IPutawayService _putawayService;

        public PutawayController(IPutawayService putawayService)
        {
            _putawayService = putawayService;
        }

        /// <summary>
        /// Belirtilen depoya ait raflanmayı (yerleştirilmeyi) bekleyen, kapı sayımı tamamlanmış ancak henüz rafa dizilmemiş belgeleri (Inbound/Transfer) listeler.
        /// </summary>
        [HttpGet("Pending/{warehouseId}")]
        public async Task<IActionResult> GetPendingPutaways(Guid warehouseId)
        {
            var list = await _putawayService.GetPendingPutawaysAsync(warehouseId);
            return Ok(CustomResponseDto<IEnumerable<PutawayListDto>>.SuccessResponse(list));
        }

        /// <summary>
        /// Raflanacak ürünlerin listesi, onaylanan miktarlar ve sistemin hacim/ağırlık kurallarına göre önerdiği hedef raflar dahil olmak üzere ilgili belgenin detaylarını getirir.
        /// </summary>
        [HttpGet("Detail/{documentType}/{documentId}")]
        public async Task<IActionResult> GetPutawayDetail(string documentType, Guid documentId)
        {
            var detail = await _putawayService.GetPutawayDetailAsync(documentId, documentType);
            return Ok(CustomResponseDto<PutawayDetailDto>.SuccessResponse(detail));
        }

        /// <summary>
        /// Saha personelinin ürünleri fiziksel olarak raflara yerleştirmesi işlemini onaylar, stok miktarlarını artırır ve raf doluluk oranlarını günceller.
        /// </summary>
        [HttpPost("Execute")]
        public async Task<IActionResult> ExecutePutaway([FromBody] PutawayRequestDto requestDto)
        {
            var result = await _putawayService.ExecutePutawayAsync(requestDto);
            return Ok(CustomResponseDto<bool>.SuccessResponse(result));
        }
    }
}