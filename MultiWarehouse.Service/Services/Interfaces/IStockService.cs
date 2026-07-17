using MultiWarehouse.Shared.DTOs.StockDtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiWarehouse.Service.Services.Interfaces
{
    public interface IStockService
    {
        Task<StockDto> CreateAsync(StockCreateDto createDto);
        Task<StockDto> GetByIdAsync(Guid id);
        Task<IEnumerable<StockDto>> GetAllAsync();

        /// <summary>Belirli bir ürüne ait tüm stok noktalarını getirir.</summary>
        Task<IEnumerable<StockDto>> GetAllByProductIdAsync(Guid productId);

        /// <summary>Belirli bir depodaki tüm stokları getirir.</summary>
        Task<IEnumerable<StockDto>> GetAllByWarehouseIdAsync(Guid warehouseId);

        /// <summary>Spesifik bir raftaki tüm stokları getirir.</summary>
        Task<IEnumerable<StockDto>> GetAllByShelfIdAsync(Guid shelfId);

        Task<StockDto> UpdateAsync(StockUpdateDto updateDto);
        Task RemoveAsync(Guid id);
    }
}