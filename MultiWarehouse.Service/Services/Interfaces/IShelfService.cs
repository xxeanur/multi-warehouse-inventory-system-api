using MultiWarehouse.Shared.DTOs.ShelfDtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiWarehouse.Service.Services.Interfaces
{
    public interface IShelfService
    {
        Task<ShelfDto> CreateAsync(ShelfCreateDto createDto);
        Task<ShelfDto> GetByIdAsync(Guid id);
        Task<IEnumerable<ShelfDto>> GetAllAsync();

        /// <summary>
        /// Belirli bir depo bloğuna (Zone) ait tüm rafları getirir.
        /// </summary>
        Task<IEnumerable<ShelfDto>> GetAllByZoneIdAsync(Guid zoneId);

        Task<ShelfDto> UpdateAsync(ShelfUpdateDto updateDto);
        Task RemoveAsync(Guid id);
    }
}