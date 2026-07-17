using MultiWarehouse.Shared.DTOs.StockMovementDtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiWarehouse.Service.Services.Interfaces
{
    public interface IStockMovementService
    {
        Task<StockMovementDto> CreateAsync(StockMovementCreateDto createDto);
        Task<StockMovementDto> GetByIdAsync(Guid id);
        Task<IEnumerable<StockMovementDto>> GetAllAsync();
        Task<IEnumerable<StockMovementDto>> GetAllByProductIdAsync(Guid productId);
        Task<IEnumerable<StockMovementDto>> GetAllByWarehouseIdAsync(Guid warehouseId);
        Task<StockMovementDto> UpdateAsync(StockMovementUpdateDto updateDto);
        Task RemoveAsync(Guid id);
    }
}