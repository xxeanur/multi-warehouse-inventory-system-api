using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiWarehouse.Service.Services.Interfaces.Definations;
using MultiWarehouse.Shared.DTOs;
using MultiWarehouse.Shared.DTOs.ProductDtos;
using MultiWarehouse.Shared.Pagination;

namespace MultiWarehouse.API.Controllers
{
    [Authorize] // Sınıf genelinde tüm kullanıcılara okuma hakkı verir
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        #region Write Operations (Only SuperAdmin)

        /// <summary>
        /// Sisteme yeni bir ürün ekler. (Sadece SuperAdmin)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Create(ProductCreateDto createDto)
        {
            var product = await _productService.CreateAsync(createDto);
            return Ok(CustomResponseDto<ProductDto>.SuccessResponse(product));
        }

        /// <summary>
        /// Mevcut bir ürünün bilgilerini günceller. (Sadece SuperAdmin)
        /// </summary>
        [HttpPut]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Update(ProductUpdateDto updateDto)
        {
            var product = await _productService.UpdateAsync(updateDto);
            return Ok(CustomResponseDto<ProductDto>.SuccessResponse(product));
        }

        /// <summary>
        /// Belirtilen ürünü sistemden siler (pasife çeker). (Sadece SuperAdmin)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Remove(Guid id)
        {
            await _productService.RemoveAsync(id);
            return Ok(CustomResponseDto.SuccessResponse());
        }

        #endregion

        #region Read Operations (All Authenticated Users)

        /// <summary>
        /// Sistemdeki tüm aktif ürünleri listeler.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllAsync();
            return Ok(CustomResponseDto<IEnumerable<ProductDto>>.SuccessResponse(products));
        }

        /// <summary>
        /// Sistemdeki ürünleri sayfalama (Pagination) destekli olarak getirir.
        /// </summary>
        [HttpGet("Paged")]
        public async Task<IActionResult> GetPaged([FromQuery] PaginationParams paginationParams)
        {
            var pagedProducts = await _productService.GetPagedAsync(paginationParams);
            return Ok(CustomResponseDto<PagedResult<ProductDto>>.SuccessResponse(pagedProducts));
        }

        /// <summary>
        /// Belirtilen ID'ye sahip ürünü detaylarıyla getirir.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var product = await _productService.GetByIdAsync(id);
            return Ok(CustomResponseDto<ProductDto>.SuccessResponse(product));
        }

        /// <summary>
        /// Barkod numarasını okutarak ilgili ürünü getirir.
        /// </summary>
        [HttpGet("barcode/{barcode}")]
        public async Task<IActionResult> GetByBarcode(string barcode)
        {
            var product = await _productService.GetByBarcodeAsync(barcode);
            return Ok(CustomResponseDto<ProductDto>.SuccessResponse(product));
        }

        /// <summary>
        /// SKU (Stok Tutma Birimi) kodunu girerek ilgili ürünü getirir.
        /// </summary>
        [HttpGet("sku/{sku}")]
        public async Task<IActionResult> GetBySku(string sku)
        {
            var product = await _productService.GetBySkuAsync(sku);
            return Ok(CustomResponseDto<ProductDto>.SuccessResponse(product));
        }

        /// <summary>
        /// İsme veya SKU'ya göre ürünlerde arama yapar (Autocomplete için kullanılır).
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            var products = await _productService.SearchAsync(query);
            return Ok(CustomResponseDto<IEnumerable<ProductDto>>.SuccessResponse(products));
        }

        /// <summary>
        /// Sadece belirtilen kategoriye (CategoryId) ait olan ürünleri listeler.
        /// </summary>
        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetByCategoryId(Guid categoryId)
        {
            var products = await _productService.GetAllByCategoryIdAsync(categoryId);
            return Ok(CustomResponseDto<IEnumerable<ProductDto>>.SuccessResponse(products));
        }

        /// <summary>
        /// Sadece belirtilen tedarikçiden (SupplierId) sağlanan ürünleri listeler.
        /// </summary>
        [HttpGet("supplier/{supplierId}")]
        public async Task<IActionResult> GetBySupplierId(Guid supplierId)
        {
            var products = await _productService.GetAllBySupplierIdAsync(supplierId);
            return Ok(CustomResponseDto<IEnumerable<ProductDto>>.SuccessResponse(products));
        }

        #endregion
    }
}