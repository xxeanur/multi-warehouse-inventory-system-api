using MultiWarehouse.Shared.DTOs.SupplierDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Service.Services.Interfaces
{
    public interface ISupplierService
    {
        Task<SupplierDto> CreateAsync(SupplierCreateDto createDto);
        Task<SupplierDto> GetByIdAsync(Guid id);
        Task<IEnumerable<SupplierDto>> GetAllAsync();
        Task<SupplierDto> UpdateAsync(SupplierUpdateDto updateDto);
        Task RemoveAsync(Guid id);
    }
}