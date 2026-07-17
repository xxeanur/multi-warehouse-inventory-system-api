using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MultiWarehouse.Entity.Entities;
using MultiWarehouse.Service.Context;
using MultiWarehouse.Service.Exceptions;
using MultiWarehouse.Service.Repositories.Interfaces;
using MultiWarehouse.Service.Services.Interfaces;
using MultiWarehouse.Shared.DTOs.WarehouseZoneDtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiWarehouse.Service.Services.Implementations
{
    public class WarehouseZoneService : IWarehouseZoneService
    {
        private readonly IGenericRepository<WarehouseZone> _zoneRepository;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public WarehouseZoneService(
            IGenericRepository<WarehouseZone> zoneRepository,
            AppDbContext context,
            IMapper mapper)
        {
            _zoneRepository = zoneRepository;
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Depo içine yeni bir blok/alan ekler.
        /// </summary>
        public async Task<WarehouseZoneDto> CreateAsync(WarehouseZoneCreateDto createDto)
        {
            createDto.ZoneName = createDto.ZoneName.Trim();

            // İş Kuralı: Depo var mı?
            var isWarehouseExists = await _context.Set<Warehouse>()
                .AnyAsync(x => x.Id == createDto.WarehouseId && x.IsActive);

            if (!isWarehouseExists)
                throw new ClientSideException("Blok eklenmek istenen depo sistemde bulunamadı.");

            // İş Kuralı: Aynı depoda aynı isimde blok var mı?
            var isZoneNameExists = await _zoneRepository.AnyAsync(x =>
                x.WarehouseId == createDto.WarehouseId &&
                x.ZoneName.ToLower() == createDto.ZoneName.ToLower() &&
                x.IsActive);

            if (isZoneNameExists)
                throw new ClientSideException("Bu deponun içinde aynı isimde bir blok zaten mevcut.");

            var zone = _mapper.Map<WarehouseZone>(createDto);

            await _zoneRepository.AddAsync(zone);
            await _context.SaveChangesAsync();

            return _mapper.Map<WarehouseZoneDto>(zone);
        }

        /// <summary>
        /// Belirtilen ID'ye sahip depo alanını getirir.
        /// </summary>
        public async Task<WarehouseZoneDto> GetByIdAsync(Guid id)
        {
            var zone = await _zoneRepository
                .Where(x => x.Id == id && x.IsActive)
                .SingleOrDefaultAsync();

            if (zone == null)
                throw new ClientSideException("Depo alanı bulunamadı.");

            return _mapper.Map<WarehouseZoneDto>(zone);
        }

        /// <summary>
        /// Tüm aktif depo alanlarını listeler.
        /// </summary>
        public async Task<IEnumerable<WarehouseZoneDto>> GetAllAsync()
        {
            var zones = await _zoneRepository
                .Where(x => x.IsActive)
                .ToListAsync();

            return _mapper.Map<IEnumerable<WarehouseZoneDto>>(zones);
        }

        /// <summary>
        /// Belirli bir depoya ait tüm aktif alanları listeler.
        /// </summary>
        public async Task<IEnumerable<WarehouseZoneDto>> GetAllByWarehouseIdAsync(Guid warehouseId)
        {
            var zones = await _zoneRepository
                .Where(x => x.WarehouseId == warehouseId && x.IsActive)
                .ToListAsync();

            return _mapper.Map<IEnumerable<WarehouseZoneDto>>(zones);
        }

        /// <summary>
        /// Depo alanını günceller.
        /// </summary>
        public async Task<WarehouseZoneDto> UpdateAsync(WarehouseZoneUpdateDto updateDto)
        {
            updateDto.ZoneName = updateDto.ZoneName.Trim();

            var zone = await _zoneRepository
                .Where(x => x.Id == updateDto.Id && x.IsActive)
                .SingleOrDefaultAsync();

            if (zone == null)
                throw new ClientSideException("Güncellenmek istenen depo alanı bulunamadı.");

            // İş Kuralı: Yeni Warehouse gerçekten var mı?
            var isWarehouseExists = await _context.Set<Warehouse>()
                .AnyAsync(x => x.Id == updateDto.WarehouseId && x.IsActive);

            if (!isWarehouseExists)
                throw new ClientSideException("Seçilen depo sistemde bulunamadı.");

            // İş Kuralı: Aynı depoda aynı isimde başka blok var mı?
            var isZoneNameExists = await _zoneRepository.AnyAsync(x =>
                x.WarehouseId == updateDto.WarehouseId &&
                x.ZoneName.ToLower() == updateDto.ZoneName.ToLower() &&
                x.Id != updateDto.Id &&
                x.IsActive);

            if (isZoneNameExists)
                throw new ClientSideException("Bu deponun içinde aynı isimde başka bir blok zaten mevcut.");

            zone.ZoneName = updateDto.ZoneName;
            zone.ZoneType = updateDto.ZoneType;
            zone.WarehouseId = updateDto.WarehouseId;

            _zoneRepository.Update(zone);
            await _context.SaveChangesAsync();

            return _mapper.Map<WarehouseZoneDto>(zone);
        }

        /// <summary>
        /// Depo alanını pasif duruma çeker (Soft Delete).
        /// </summary>
        public async Task RemoveAsync(Guid id)
        {
            var zone = await _zoneRepository
                .Where(x => x.Id == id && x.IsActive)
                .SingleOrDefaultAsync();

            if (zone == null)
                throw new ClientSideException("Silinmek istenen depo alanı bulunamadı.");

            // İleride Shelf tablosu geldiğinde burada
            // bu alana ait raf var mı kontrolü eklenebilir.

            zone.IsActive = false;

            _zoneRepository.Update(zone);
            await _context.SaveChangesAsync();
        }
    }
}