using MultiWarehouse.Shared.DTOs.WarehouseDtos;
using MultiWarehouse.Shared.Pagination;

namespace MultiWarehouse.Service.Services.Interfaces
{
    public interface IWarehouseService
    {
        Task<WarehouseDto> CreateAsync(WarehouseCreateDto createDto);
        Task<WarehouseDto> GetByIdAsync(Guid id);
        Task<IEnumerable<WarehouseDto>> GetAllAsync();
        Task<PagedResult<WarehouseDto>> GetPagedAsync(PaginationParams paginationParams);//pagination
        Task<WarehouseDto> UpdateAsync(WarehouseUpdateDto updateDto);
        Task RemoveAsync(Guid id);
    }
}