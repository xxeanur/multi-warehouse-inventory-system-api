using MultiWarehouse.Shared.DTOs.ProductDtos;
using MultiWarehouse.Shared.Pagination;

namespace MultiWarehouse.Service.Services.Interfaces
{
    public interface IProductService
    {
        Task<ProductDto> CreateAsync(ProductCreateDto createDto);
        Task<ProductDto> GetByIdAsync(Guid id);
        Task<IEnumerable<ProductDto>> GetAllAsync();

        //pagination
        Task<PagedResult<ProductDto>> GetPagedAsync(PaginationParams paginationParams);

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