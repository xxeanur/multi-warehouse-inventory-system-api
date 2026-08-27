using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiWarehouse.Service.Services.Interfaces.Common;
using MultiWarehouse.Shared.DTOs; // Senin DTO'nun olduğu namespace
using MultiWarehouse.Shared.DTOs.SearchDtos;

namespace MultiWarehouse.API.Controllers
{
    /// <summary>
    /// Sistem genelinde (Global) arama işlemlerini yürüten API kontrolcüsü.
    /// WMS içerisindeki belgeleri, ürünleri ve depoları tek bir noktadan sorgular.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        #region QUERY İŞLEMLERİ (GET)

        /// <summary>
        /// Kullanıcının girdiği kelimeye göre sistemdeki tüm modüllerde (Ürün, Mal Kabul, Sevkiyat, Depo) arama yapar.
        /// </summary>
        /// <param name="q">Aranacak kelime (En az 2 karakter olmalıdır).</param>
        /// <returns>Kategorize edilmiş arama sonuç listesi.</returns>
        [HttpGet]
        public async Task<IActionResult> GlobalSearch([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            {
                return Ok(CustomResponseDto<IEnumerable<SearchResultItemDto>>.SuccessResponse(new List<SearchResultItemDto>()));
            }

            var results = await _searchService.SearchAcrossModulesAsync(q);

            return Ok(CustomResponseDto<IEnumerable<SearchResultItemDto>>.SuccessResponse(results));
        }

        #endregion
    }
}