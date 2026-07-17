using MultiWarehouse.Shared.DTOs.WarehouseDtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiWarehouse.Service.Services.Interfaces
{
    public interface IWarehouseService
    {
        Task<WarehouseDto> CreateAsync(WarehouseCreateDto createDto);
        Task<WarehouseDto> GetByIdAsync(Guid id);
        Task<IEnumerable<WarehouseDto>> GetAllAsync();
        Task<WarehouseDto> UpdateAsync(WarehouseUpdateDto updateDto);
        Task RemoveAsync(Guid id);
    }
}