using MultiWarehouse.Shared.DTOs.WarehouseZoneDtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiWarehouse.Service.Services.Interfaces
{
    public interface IWarehouseZoneService
    {
        Task<WarehouseZoneDto> CreateAsync(WarehouseZoneCreateDto createDto);
        Task<WarehouseZoneDto> GetByIdAsync(Guid id);
        Task<IEnumerable<WarehouseZoneDto>> GetAllAsync();

        /// <summary>
        /// Belirli bir depoya ait tüm blokları/alanları getirir.
        /// </summary>
        Task<IEnumerable<WarehouseZoneDto>> GetAllByWarehouseIdAsync(Guid warehouseId);

        Task<WarehouseZoneDto> UpdateAsync(WarehouseZoneUpdateDto updateDto);
        Task RemoveAsync(Guid id);
    }
}