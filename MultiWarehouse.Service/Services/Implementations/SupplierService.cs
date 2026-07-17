using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MultiWarehouse.Entity.Entities;
using MultiWarehouse.Service.Context;
using MultiWarehouse.Service.Exceptions;
using MultiWarehouse.Service.Repositories.Interfaces;
using MultiWarehouse.Service.Services.Interfaces;
using MultiWarehouse.Shared.DTOs.SupplierDtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiWarehouse.Service.Services.Implementations
{
    public class SupplierService : ISupplierService
    {
        private readonly IGenericRepository<Supplier> _supplierRepository;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public SupplierService(IGenericRepository<Supplier> supplierRepository, AppDbContext context, IMapper mapper)
        {
            _supplierRepository = supplierRepository;
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Yeni bir tedarikçi oluşturur.
        /// Vergi numarası, E-posta ve Telefon numarası benzersizlik kontrollerinden geçer.
        /// </summary>
        public async Task<SupplierDto> CreateAsync(SupplierCreateDto createDto)
        {
            // İş Kuralı 1: Aynı vergi numarasına sahip tedarikçi var mı?
            var isTaxNumberExists = await _supplierRepository.AnyAsync(x => x.TaxNumber == createDto.TaxNumber && x.IsActive);
            if (isTaxNumberExists)
                throw new ClientSideException("Bu vergi numarasına kayıtlı bir tedarikçi zaten mevcut.");

            // İş Kuralı 2: Aynı E-posta adresine sahip tedarikçi var mı?
            var isEmailExists = await _supplierRepository.AnyAsync(x => x.Email.ToLower().Trim() == createDto.Email.ToLower().Trim() && x.IsActive);
            if (isEmailExists)
                throw new ClientSideException("Bu e-posta adresi başka bir tedarikçi tarafından kullanılıyor.");

            var supplier = _mapper.Map<Supplier>(createDto);

            await _supplierRepository.AddAsync(supplier);
            await _context.SaveChangesAsync();

            return _mapper.Map<SupplierDto>(supplier);
        }

        /// <summary>
        /// Belirtilen ID'ye sahip aktif tedarikçiyi getirir.
        /// </summary>
        public async Task<SupplierDto> GetByIdAsync(Guid id)
        {
            var supplier = await _supplierRepository.Where(x => x.Id == id && x.IsActive).SingleOrDefaultAsync();
            if (supplier == null)
                throw new ClientSideException("Tedarikçi bulunamadı.");

            return _mapper.Map<SupplierDto>(supplier);
        }

        /// <summary>
        /// Sistemdeki tüm aktif tedarikçileri listeler.
        /// </summary>
        public async Task<IEnumerable<SupplierDto>> GetAllAsync()
        {
            var suppliers = await _supplierRepository.Where(x => x.IsActive).ToListAsync();
            return _mapper.Map<IEnumerable<SupplierDto>>(suppliers);
        }

        /// <summary>
        /// Mevcut bir tedarikçinin bilgilerini günceller.
        /// Güncellenen vergi numarası, e-posta ve telefonun başka bir firmayla çakışıp çakışmadığını kontrol eder.
        /// </summary>
        public async Task<SupplierDto> UpdateAsync(SupplierUpdateDto updateDto)
        {
            var supplier = await _supplierRepository.Where(x => x.Id == updateDto.Id && x.IsActive).SingleOrDefaultAsync();
            if (supplier == null)
                throw new ClientSideException("Güncellenmek istenen tedarikçi bulunamadı.");

            // İş Kuralı 1: Güncellenen vergi numarası başka bir firmaya ait mi?
            var isTaxNumberExists = await _supplierRepository.AnyAsync(x => x.TaxNumber == updateDto.TaxNumber && x.Id != updateDto.Id && x.IsActive);
            if (isTaxNumberExists)
                throw new ClientSideException("Girdiğiniz vergi numarası sistemde başka bir tedarikçi tarafından kullanılıyor.");

            // İş Kuralı 2: Güncellenen e-posta başka bir firmaya ait mi?
            var isEmailExists = await _supplierRepository.AnyAsync(x => x.Email.ToLower().Trim() == updateDto.Email.ToLower().Trim() && x.Id != updateDto.Id && x.IsActive);
            if (isEmailExists)
                throw new ClientSideException("Girdiğiniz e-posta adresi sistemde başka bir tedarikçi tarafından kullanılıyor.");


            // Mapleme yerine manuel atama (Performans ve güvenlik için daha kontrollü)
            supplier.CompanyName = updateDto.CompanyName;
            supplier.ContactName = updateDto.ContactName;
            supplier.Email = updateDto.Email;
            supplier.Phone = updateDto.Phone;
            supplier.Address = updateDto.Address;
            supplier.TaxNumber = updateDto.TaxNumber;
            supplier.TaxOffice = updateDto.TaxOffice;

            _supplierRepository.Update(supplier);
            await _context.SaveChangesAsync();

            return _mapper.Map<SupplierDto>(supplier);
        }

        /// <summary>
        /// Belirtilen tedarikçiyi pasif (soft delete) duruma çeker.
        /// </summary>
        public async Task RemoveAsync(Guid id)
        {
            var supplier = await _supplierRepository.Where(x => x.Id == id && x.IsActive).SingleOrDefaultAsync();
            if (supplier == null)
                throw new ClientSideException("Silinmek istenen tedarikçi bulunamadı.");

            supplier.IsActive = false; // Soft Delete

            _supplierRepository.Update(supplier);
            await _context.SaveChangesAsync();
        }
    }
}