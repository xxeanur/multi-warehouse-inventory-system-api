using MultiWarehouse.Shared.DTOs.WarehouseZoneDtos;

namespace MultiWarehouse.Service.Services.Interfaces.Definations
{
    public interface IWarehouseZoneService
    {
        #region Write Operations

        /// <summary>
        /// Depo içine yeni bir blok/alan ekler. (Sadece SuperAdmin ve Kendi Deposundaki Manager yapabilir)
        /// </summary>
        Task<WarehouseZoneDto> CreateAsync(WarehouseZoneCreateDto createDto);

        /// <summary>
        /// Mevcut bir depo alanını günceller. (Sadece SuperAdmin ve Kendi Deposundaki Manager yapabilir)
        /// </summary>
        Task<WarehouseZoneDto> UpdateAsync(WarehouseZoneUpdateDto updateDto);

        /// <summary>
        /// Depo alanını sistemden siler (pasife çeker). (Sadece SuperAdmin ve Kendi Deposundaki Manager yapabilir)
        /// </summary>
        Task RemoveAsync(Guid id);

        #endregion

        #region Read Operations

        /// <summary>
        /// Belirtilen ID'ye sahip depo alanını getirir. (Kullanıcı sadece yetkili olduğu depodakini görebilir)
        /// </summary>
        Task<WarehouseZoneDto> GetByIdAsync(Guid id);

        /// <summary>
        /// Tüm aktif depo alanlarını listeler. (Satır bazlı güvenlik: Yetkili olunan depoları filtreler)
        /// </summary>
        Task<IEnumerable<WarehouseZoneDto>> GetAllAsync();

        /// <summary>
        /// Belirli bir depoya ait tüm blokları/alanları getirir.
        /// </summary>
        Task<IEnumerable<WarehouseZoneDto>> GetAllByWarehouseIdAsync(Guid warehouseId);

        #endregion
    }
}