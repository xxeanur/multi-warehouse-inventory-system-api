using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MultiWarehouse.Entity.Entities;
using MultiWarehouse.Service.Context;
using MultiWarehouse.Service.Exceptions;
using MultiWarehouse.Service.Repositories.Interfaces;
using MultiWarehouse.Service.Services.Interfaces;
using MultiWarehouse.Shared.DTOs.WarehouseDtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiWarehouse.Service.Services.Implementations
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IGenericRepository<Warehouse> _warehouseRepository;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public WarehouseService(IGenericRepository<Warehouse> warehouseRepository, AppDbContext context, IMapper mapper)
        {
            _warehouseRepository = warehouseRepository;
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Yeni bir depo oluşturur. 
        /// Başlangıçta kullanılan kapasite (UsedCapacity) 0 olarak ayarlanır.
        /// </summary>
        public async Task<WarehouseDto> CreateAsync(WarehouseCreateDto createDto)
        {
            // İş Kuralı: Aynı isimde aktif bir depo var mı?
            var isNameExists = await _warehouseRepository.AnyAsync(x => x.Name.ToLower() == createDto.Name.ToLower() && x.IsActive);
            if (isNameExists)
            {
                throw new ClientSideException("Bu isimde bir depo zaten mevcut.");
            }

            var warehouse = _mapper.Map<Warehouse>(createDto);
            warehouse.UsedCapacity = 0; // Yeni deponun içi boştur

            await _warehouseRepository.AddAsync(warehouse);
            await _context.SaveChangesAsync();

            return _mapper.Map<WarehouseDto>(warehouse);
        }

        /// <summary>
        /// Belirtilen ID'ye sahip aktif depoyu getirir.
        /// </summary>
        public async Task<WarehouseDto> GetByIdAsync(Guid id)
        {
            var warehouse = await _warehouseRepository.Where(x => x.Id == id && x.IsActive).SingleOrDefaultAsync();
            if (warehouse == null)
            {
                throw new ClientSideException("Depo bulunamadı.");
            }

            return _mapper.Map<WarehouseDto>(warehouse);
        }

        /// <summary>
        /// Sistemdeki tüm aktif depoları listeler.
        /// </summary>
        public async Task<IEnumerable<WarehouseDto>> GetAllAsync()
        {
            var warehouses = await _warehouseRepository.Where(x => x.IsActive).ToListAsync();
            return _mapper.Map<IEnumerable<WarehouseDto>>(warehouses);
        }

        /// <summary>
        /// Mevcut bir deponun temel bilgilerini günceller.
        /// Depo adının başka bir depoyla çakışıp çakışmadığını kontrol eder.
        /// </summary>
        public async Task<WarehouseDto> UpdateAsync(WarehouseUpdateDto updateDto)
        {
            var warehouse = await _warehouseRepository.Where(x => x.Id == updateDto.Id && x.IsActive).SingleOrDefaultAsync();
            if (warehouse == null)
            {
                throw new ClientSideException("Güncellenmek istenen depo bulunamadı.");
            }

            // İş Kuralı: Güncellenen isim başka bir depoya ait mi?
            var isNameExists = await _warehouseRepository.AnyAsync(x => x.Name.ToLower() == updateDto.Name.ToLower() && x.Id != updateDto.Id && x.IsActive);
            if (isNameExists)
            {
                throw new ClientSideException("Bu depo adı sistemde başka bir depo tarafından kullanılıyor.");
            }

            warehouse.Name = updateDto.Name;
            warehouse.Location = updateDto.Location;
            warehouse.Phone = updateDto.Phone;
            warehouse.ManagerId = updateDto.ManagerId;

            // Eğer kapasite düşürülüyorsa ve içerideki mal (UsedCapacity) yeni MaxCapacity'den fazlaysa hata fırlatılabilir.
            if (updateDto.MaxCapacity < warehouse.UsedCapacity)
            {
                throw new ClientSideException($"Deponun maksimum kapasitesi, mevcut doluluk oranından ({warehouse.UsedCapacity}) küçük olamaz.");
            }

            warehouse.MaxCapacity = updateDto.MaxCapacity;

            _warehouseRepository.Update(warehouse);
            await _context.SaveChangesAsync();

            return _mapper.Map<WarehouseDto>(warehouse);
        }

        /// <summary>
        /// Belirtilen depoyu pasif (soft delete) duruma çeker.
        /// </summary>
        public async Task RemoveAsync(Guid id)
        {
            var warehouse = await _warehouseRepository.Where(x => x.Id == id && x.IsActive).SingleOrDefaultAsync();
            if (warehouse == null)
            {
                throw new ClientSideException("Silinmek istenen depo bulunamadı.");
            }

            // İş Kuralı: İçi dolu olan depo silinemez
            if (warehouse.UsedCapacity > 0)
            {
                throw new ClientSideException("İçerisinde ürün bulunan bir depo silinemez. Önce stokları transfer etmelisiniz.");
            }

            warehouse.IsActive = false; // Soft Delete

            _warehouseRepository.Update(warehouse);
            await _context.SaveChangesAsync();
        }
    }
}