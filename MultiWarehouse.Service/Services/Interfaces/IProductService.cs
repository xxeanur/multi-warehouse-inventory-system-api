using MultiWarehouse.Shared.DTOs.ProductDtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiWarehouse.Service.Services.Interfaces
{
    public interface IProductService
    {
        Task<ProductDto> CreateAsync(ProductCreateDto createDto);
        Task<ProductDto> GetByIdAsync(Guid id);
        Task<IEnumerable<ProductDto>> GetAllAsync();

        /// <summary>
        /// Belirli bir kategoriye ait ürünleri listeler.
        /// </summary>
        Task<IEnumerable<ProductDto>> GetAllByCategoryIdAsync(Guid categoryId);

        /// <summary>
        /// Belirli bir tedarikçiden alınan ürünleri listeler.
        /// </summary>
        Task<IEnumerable<ProductDto>> GetAllBySupplierIdAsync(Guid supplierId);

        //barkodla ürün getirme
        Task<ProductDto> GetByBarcodeAsync(string barcode);

        //sku ile ürün getirme
        Task<ProductDto> GetBySkuAsync(string sku);

        Task<ProductDto> UpdateAsync(ProductUpdateDto updateDto);
        Task RemoveAsync(Guid id);
    }
}