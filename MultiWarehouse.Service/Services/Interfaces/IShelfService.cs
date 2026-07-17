using MultiWarehouse.Shared.DTOs.ShelfDtos;
using MultiWarehouse.Shared.Pagination;

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

        //pagination
        // Tüm rafları sayfalayarak getirir
        Task<PagedResult<ShelfDto>> GetPagedAsync(PaginationParams paginationParams);

        // Belirli bir bloğa (Zone) ait rafları sayfalayarak getirir
        Task<PagedResult<ShelfDto>> GetPagedByZoneIdAsync(PaginationParams paginationParams, Guid zoneId);

        Task<ShelfDto> UpdateAsync(ShelfUpdateDto updateDto);
        Task RemoveAsync(Guid id);
    }
}