using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiWarehouse.Service.Services.Interfaces;
using MultiWarehouse.Shared.DTOs;
using MultiWarehouse.Shared.DTOs.ProductDtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiWarehouse.API.Controllers
{
    [Authorize(Roles = "SuperAdmin,WarehouseManager")]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Sisteme yeni bir ürün ekler.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(ProductCreateDto createDto)
        {
            var product = await _productService.CreateAsync(createDto);
            return Ok(CustomResponseDto<ProductDto>.SuccessResponse(product));
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
        [HttpGet("GetByBarcode/{barcode}")]
        public async Task<IActionResult> GetByBarcode(string barcode)
        {
            var product = await _productService.GetByBarcodeAsync(barcode);
            return Ok(CustomResponseDto<ProductDto>.SuccessResponse(product));
        }

        /// <summary>
        /// SKU (Stok Tutma Birimi) kodunu girerek ilgili ürünü getirir.
        /// </summary>
        [HttpGet("GetBySku/{sku}")]
        public async Task<IActionResult> GetBySku(string sku)
        {
            var product = await _productService.GetBySkuAsync(sku);
            return Ok(CustomResponseDto<ProductDto>.SuccessResponse(product));
        }

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
        /// Sadece belirtilen kategoriye (CategoryId) ait olan ürünleri listeler.
        /// </summary>
        [HttpGet("GetByCategoryId/{categoryId}")]
        public async Task<IActionResult> GetByCategoryId(Guid categoryId)
        {
            var products = await _productService.GetAllByCategoryIdAsync(categoryId);
            return Ok(CustomResponseDto<IEnumerable<ProductDto>>.SuccessResponse(products));
        }

        /// <summary>
        /// Sadece belirtilen tedarikçiden (SupplierId) sağlanan ürünleri listeler.
        /// </summary>
        [HttpGet("GetBySupplierId/{supplierId}")]
        public async Task<IActionResult> GetBySupplierId(Guid supplierId)
        {
            var products = await _productService.GetAllBySupplierIdAsync(supplierId);
            return Ok(CustomResponseDto<IEnumerable<ProductDto>>.SuccessResponse(products));
        }

        /// <summary>
        /// Mevcut bir ürünün bilgilerini günceller.
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> Update(ProductUpdateDto updateDto)
        {
            var product = await _productService.UpdateAsync(updateDto);
            return Ok(CustomResponseDto<ProductDto>.SuccessResponse(product));
        }

        /// <summary>
        /// Belirtilen ürünü sistemden siler (pasife çeker).
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Remove(Guid id)
        {
            await _productService.RemoveAsync(id);
            return Ok(CustomResponseDto.SuccessResponse());
        }
    }
}