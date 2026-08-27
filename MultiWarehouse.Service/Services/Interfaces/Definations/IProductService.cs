using MultiWarehouse.Shared.DTOs.ProductDtos;
using MultiWarehouse.Shared.Pagination;

namespace MultiWarehouse.Service.Services.Interfaces.Definations
{
    public interface IProductService
    {
        #region Write Operations

        /// <summary>
        /// Yeni bir ürün oluşturur. (Master Data - Sadece SuperAdmin)
        /// </summary>
        Task<ProductDto> CreateAsync(ProductCreateDto createDto);

        /// <summary>
        /// Mevcut ürünü günceller. (Master Data - Sadece SuperAdmin)
        /// </summary>
        Task<ProductDto> UpdateAsync(ProductUpdateDto updateDto);

        /// <summary>
        /// Ürünü pasif duruma alır. (Master Data - Sadece SuperAdmin)
        /// </summary>
        Task RemoveAsync(Guid id);

        #endregion

        #region Read Operations

        /// <summary>
        /// Belirtilen ID'ye sahip ürünü detaylarıyla getirir.
        /// </summary>
        Task<ProductDto> GetByIdAsync(Guid id);

        /// <summary>
        /// Tüm aktif ürünleri listeler.
        /// </summary>
        Task<IEnumerable<ProductDto>> GetAllAsync();

        /// <summary>
        /// Ürünleri sayfalama (Pagination) destekli getirir.
        /// </summary>
        Task<PagedResult<ProductDto>> GetPagedAsync(PaginationParams paginationParams);

        /// <summary>
        /// Belirli bir kategoriye ait ürünleri listeler.
        /// </summary>
        Task<IEnumerable<ProductDto>> GetAllByCategoryIdAsync(Guid categoryId);

        /// <summary>
        /// Belirli bir tedarikçiden alınan ürünleri listeler.
        /// </summary>
        Task<IEnumerable<ProductDto>> GetAllBySupplierIdAsync(Guid supplierId);

        /// <summary>
        /// Barkod ile ürün arar.
        /// </summary>
        Task<ProductDto> GetByBarcodeAsync(string barcode);

        /// <summary>
        /// SKU kodu ile ürün arar.
        /// </summary>
        Task<ProductDto> GetBySkuAsync(string sku);

        /// <summary>
        /// İsme veya SKU'ya göre ürün arar (Autocomplete vb. için).
        /// </summary>
        Task<IEnumerable<ProductDto>> SearchAsync(string query);

        #endregion
    }
}