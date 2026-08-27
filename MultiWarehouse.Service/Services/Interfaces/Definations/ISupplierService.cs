using MultiWarehouse.Shared.DTOs.SupplierDtos;

namespace MultiWarehouse.Service.Services.Interfaces.Definations
{
    public interface ISupplierService
    {
        #region Write Operations

        /// <summary>
        /// Yeni bir tedarikçi oluşturur. (Sadece SuperAdmin)
        /// </summary>
        Task<SupplierDto> CreateAsync(SupplierCreateDto createDto);

        /// <summary>
        /// Mevcut bir tedarikçinin bilgilerini günceller. (Sadece SuperAdmin)
        /// </summary>
        Task<SupplierDto> UpdateAsync(SupplierUpdateDto updateDto);

        /// <summary>
        /// Belirtilen tedarikçiyi pasif (soft delete) duruma çeker. (Sadece SuperAdmin)
        /// </summary>
        Task RemoveAsync(Guid id);

        #endregion

        #region Read Operations

        /// <summary>
        /// Belirtilen ID'ye sahip aktif tedarikçiyi getirir.
        /// </summary>
        Task<SupplierDto> GetByIdAsync(Guid id);

        /// <summary>
        /// Sistemdeki tüm aktif tedarikçileri listeler.
        /// </summary>
        Task<IEnumerable<SupplierDto>> GetAllAsync();

        #endregion
    }
}