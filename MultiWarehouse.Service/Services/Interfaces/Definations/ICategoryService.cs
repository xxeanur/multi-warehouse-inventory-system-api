using MultiWarehouse.Shared.DTOs.CategoryDtos;

namespace MultiWarehouse.Service.Services.Interfaces.Definations
{
    public interface ICategoryService
    {
        #region Category Operations

        /// <summary>
        /// Yeni kategori oluşturur. (Global Master Data)
        /// </summary>
        Task<CategoryDto> CreateAsync(CategoryCreateDto createDto);

        /// <summary>
        /// Belirtilen ID'ye sahip aktif kategoriyi getirir.
        /// </summary>
        Task<CategoryDto> GetByIdAsync(Guid id);

        /// <summary>
        /// Tüm aktif kategorileri listeler.
        /// </summary>
        Task<IEnumerable<CategoryDto>> GetAllAsync();

        /// <summary>
        /// Mevcut kategoriyi günceller.
        /// </summary>
        Task<CategoryDto> UpdateAsync(CategoryUpdateDto updateDto);

        /// <summary>
        /// Belirtilen kategoriyi pasif (soft delete) duruma çeker.
        /// </summary>
        Task RemoveAsync(Guid id);

        #endregion
    }
}