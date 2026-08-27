using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MultiWarehouse.Entity.Entities.Definitions;
using MultiWarehouse.Service.Exceptions;
using MultiWarehouse.Service.Repositories.Interfaces;
using MultiWarehouse.Service.Services.Interfaces.Definations;
using MultiWarehouse.Shared.DTOs.SupplierDtos;

namespace MultiWarehouse.Service.Services.Implementations.Definitions
{
    public class SupplierService : ISupplierService
    {
        private readonly IGenericRepository<Supplier> _supplierRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SupplierService(IGenericRepository<Supplier> supplierRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _supplierRepository = supplierRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        #region Write Operations

        public async Task<SupplierDto> CreateAsync(SupplierCreateDto createDto)
        {

            createDto.CompanyName = createDto.CompanyName?.Trim().ToLower();
            createDto.ContactName = createDto.ContactName?.Trim().ToLower();
            createDto.Email = createDto.Email?.Trim().ToLower();
            createDto.TaxOffice = createDto.TaxOffice?.Trim().ToLower();
            createDto.TaxNumber = createDto.TaxNumber?.Trim();
            createDto.FullAddress = createDto.FullAddress?.Trim();
            createDto.Phone = FormatPhoneNumber(createDto.Phone);

            var isTaxNumberExists = await _supplierRepository.AnyAsync(x => x.TaxNumber == createDto.TaxNumber && x.IsActive);
            if (isTaxNumberExists)
                throw new ClientSideException("Bu vergi numarasına kayıtlı bir tedarikçi zaten mevcut.");

            var isEmailExists = await _supplierRepository.AnyAsync(x => x.Email == createDto.Email && x.IsActive);
            if (isEmailExists)
                throw new ClientSideException("Bu e-posta adresi başka bir tedarikçi tarafından kullanılıyor.");

            var supplier = _mapper.Map<Supplier>(createDto);

            await _supplierRepository.AddAsync(supplier);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<SupplierDto>(supplier);
        }

        public async Task<SupplierDto> UpdateAsync(SupplierUpdateDto updateDto)
        {
            var supplier = await _supplierRepository.Where(x => x.Id == updateDto.Id && x.IsActive).SingleOrDefaultAsync();
            if (supplier == null)
                throw new ClientSideException("Güncellenmek istenen tedarikçi bulunamadı.");

            updateDto.CompanyName = updateDto.CompanyName?.Trim().ToLower();
            updateDto.ContactName = updateDto.ContactName?.Trim().ToLower();
            updateDto.Email = updateDto.Email?.Trim().ToLower();
            updateDto.TaxOffice = updateDto.TaxOffice?.Trim().ToLower();
            updateDto.TaxNumber = updateDto.TaxNumber?.Trim();
            updateDto.FullAddress = updateDto.FullAddress?.Trim();
            updateDto.Phone = FormatPhoneNumber(updateDto.Phone);


            var isTaxNumberExists = await _supplierRepository.AnyAsync(x => x.TaxNumber == updateDto.TaxNumber && x.Id != updateDto.Id && x.IsActive);
            if (isTaxNumberExists)
                throw new ClientSideException("Girdiğiniz vergi numarası sistemde başka bir tedarikçi tarafından kullanılıyor.");

            var isEmailExists = await _supplierRepository.AnyAsync(x => x.Email == updateDto.Email && x.Id != updateDto.Id && x.IsActive);
            if (isEmailExists)
                throw new ClientSideException("Girdiğiniz e-posta adresi sistemde başka bir tedarikçi tarafından kullanılıyor.");

            supplier.CompanyName = updateDto.CompanyName;
            supplier.ContactName = updateDto.ContactName;
            supplier.Email = updateDto.Email;
            supplier.Phone = updateDto.Phone;
            supplier.Country = updateDto.Country;
            supplier.City = updateDto.City;
            supplier.District = updateDto.District;
            supplier.FullAddress = updateDto.FullAddress;
            supplier.Latitude = updateDto.Latitude;
            supplier.Longitude = updateDto.Longitude;
            supplier.TaxNumber = updateDto.TaxNumber;
            supplier.TaxOffice = updateDto.TaxOffice;

            _supplierRepository.Update(supplier);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<SupplierDto>(supplier);
        }

        public async Task RemoveAsync(Guid id)
        {
            var supplier = await _supplierRepository.Where(x => x.Id == id && x.IsActive).SingleOrDefaultAsync();
            if (supplier == null)
                throw new ClientSideException("Silinmek istenen tedarikçi bulunamadı.");

            supplier.IsActive = false;

            _supplierRepository.Update(supplier);
            await _unitOfWork.SaveChangesAsync();
        }

        #endregion

        #region Read Operations

        public async Task<SupplierDto> GetByIdAsync(Guid id)
        {
            var supplier = await _supplierRepository.Where(x => x.Id == id && x.IsActive).SingleOrDefaultAsync();
            if (supplier == null)
                throw new ClientSideException("Tedarikçi bulunamadı.");

            return _mapper.Map<SupplierDto>(supplier);
        }

        public async Task<IEnumerable<SupplierDto>> GetAllAsync()
        {
            var suppliers = await _supplierRepository.Where(x => x.IsActive).ToListAsync();
            return _mapper.Map<IEnumerable<SupplierDto>>(suppliers);
        }

        #endregion

        #region Private Helpers


        private string FormatPhoneNumber(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return string.Empty;

            var digitsOnly = new string(phone.Where(char.IsDigit).ToArray());

            if (string.IsNullOrWhiteSpace(digitsOnly))
                return string.Empty;

            if (digitsOnly.StartsWith("0"))
            {
                digitsOnly = "90" + digitsOnly.Substring(1);
            }
            else if (!digitsOnly.StartsWith("90"))
            {
                digitsOnly = "90" + digitsOnly;
            }

            return "+" + digitsOnly;
        }

        #endregion
    }
}