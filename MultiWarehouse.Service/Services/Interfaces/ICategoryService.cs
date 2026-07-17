using MultiWarehouse.Shared.DTOs.CategoryDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Service.Services.Interfaces
{
    public interface ICategoryService
    {
        /// <summary>
        /// Yeni kategori oluşturur.
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
        /// Kategoriyi günceller.
        /// </summary>
        Task<CategoryDto> UpdateAsync(CategoryUpdateDto updateDto);

        /// <summary>
        /// Belirtilen kategoriyi pasif (soft delete) duruma çeker.
        /// </summary>
        Task RemoveAsync(Guid id);
    }
}