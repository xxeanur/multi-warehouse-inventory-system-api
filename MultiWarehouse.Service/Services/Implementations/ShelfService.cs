using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MultiWarehouse.Entity.Entities;
using MultiWarehouse.Service.Context;
using MultiWarehouse.Service.Exceptions;
using MultiWarehouse.Service.Repositories.Interfaces;
using MultiWarehouse.Service.Services.Interfaces;
using MultiWarehouse.Shared.DTOs.ShelfDtos;
using MultiWarehouse.Shared.Pagination;

namespace MultiWarehouse.Service.Services.Implementations
{
    public class ShelfService : IShelfService
    {
        private readonly IGenericRepository<Shelf> _shelfRepository;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ShelfService(IGenericRepository<Shelf> shelfRepository, AppDbContext context, IMapper mapper)
        {
            _shelfRepository = shelfRepository;
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Bloğa yeni bir raf ekler.
        /// Eklenmek istenen bloğun varlığını ve raf kodunun o blokta benzersiz olup olmadığını kontrol eder.
        /// </summary>
        public async Task<ShelfDto> CreateAsync(ShelfCreateDto createDto)
        {
            // İş Kuralı 1: Rafın ekleneceği Zone (Blok) gerçekten var mı?
            var isZoneExists = await _context.Set<WarehouseZone>().AnyAsync(z => z.Id == createDto.WarehouseZoneId && z.IsActive);
            if (!isZoneExists)
                throw new ClientSideException("Raf eklenmek istenen blok/alan sistemde bulunamadı.");

            // İş Kuralı 2: Aynı bloğun içinde aynı koda sahip (Örn: A-01) başka raf var mı?
            var isShelfExists = await _shelfRepository.AnyAsync(s => s.WarehouseZoneId == createDto.WarehouseZoneId && s.ShelfNumber.ToLower() == createDto.ShelfNumber.ToLower() && s.IsActive);
            if (isShelfExists)
                throw new ClientSideException("Bu bloğun içinde aynı koda sahip bir raf zaten mevcut.");

            var shelf = _mapper.Map<Shelf>(createDto);

            // Yeni raf tamamen boş olarak sisteme girer
            shelf.CurrentVolume = 0;
            shelf.CurrentWeight = 0;

            await _shelfRepository.AddAsync(shelf);
            await _context.SaveChangesAsync();

            return _mapper.Map<ShelfDto>(shelf);
        }

        /// <summary>
        /// Belirtilen ID'ye sahip rafı getirir.
        /// </summary>
        public async Task<ShelfDto> GetByIdAsync(Guid id)
        {
            var shelf = await _shelfRepository.Where(s => s.Id == id && s.IsActive).SingleOrDefaultAsync();
            if (shelf == null)
                throw new ClientSideException("Raf bulunamadı.");

            return _mapper.Map<ShelfDto>(shelf);
        }

        /// <summary>
        /// Tüm sistemdeki rafları listeler.
        /// </summary>
        public async Task<IEnumerable<ShelfDto>> GetAllAsync()
        {
            var shelves = await _shelfRepository.Where(s => s.IsActive).ToListAsync();
            return _mapper.Map<IEnumerable<ShelfDto>>(shelves);
        }

        /// <summary>
        /// Sadece belirli bir bloğa (WarehouseZoneId) ait olan rafları listeler.
        /// </summary>
        public async Task<IEnumerable<ShelfDto>> GetAllByZoneIdAsync(Guid zoneId)
        {
            var shelves = await _shelfRepository.Where(s => s.WarehouseZoneId == zoneId && s.IsActive).ToListAsync();
            return _mapper.Map<IEnumerable<ShelfDto>>(shelves);
        }

        /// <summary>
        /// Sistemdeki tüm aktif rafları sayfalama altyapısı ile getirir.
        /// </summary>
        public async Task<PagedResult<ShelfDto>> GetPagedAsync(PaginationParams paginationParams)
        {
            var pagedEntities = await _shelfRepository.GetPagedAsync(
                paginationParams,
                filter: s => s.IsActive
            );

            return _mapper.Map<PagedResult<ShelfDto>>(pagedEntities);
        }

        /// <summary>
        /// Sadece belirli bir bloğa (Zone) ait olan aktif rafları sayfalayarak getirir.
        /// </summary>
        public async Task<PagedResult<ShelfDto>> GetPagedByZoneIdAsync(PaginationParams paginationParams, Guid zoneId)
        {
            var pagedEntities = await _shelfRepository.GetPagedAsync(
                paginationParams,
                // İşte Generic Repository'nin gücü: Filtreyi dinamik olarak genişlettik!
                filter: s => s.IsActive && s.WarehouseZoneId == zoneId
            );

            return _mapper.Map<PagedResult<ShelfDto>>(pagedEntities);
        }

        /// <summary>
        /// Mevcut bir rafın fiziksel özelliklerini, limitlerini ve durumunu günceller.
        /// </summary>
        public async Task<ShelfDto> UpdateAsync(ShelfUpdateDto updateDto)
        {
            var shelf = await _shelfRepository.Where(s => s.Id == updateDto.Id && s.IsActive).SingleOrDefaultAsync();
            if (shelf == null)
                throw new ClientSideException("Güncellenmek istenen raf bulunamadı.");

            // İş Kuralı 1: Zone değişiyorsa, yeni Zone var mı kontrolü
            if (shelf.WarehouseZoneId != updateDto.WarehouseZoneId)
            {
                var isZoneExists = await _context.Set<WarehouseZone>().AnyAsync(z => z.Id == updateDto.WarehouseZoneId && z.IsActive);
                if (!isZoneExists)
                    throw new ClientSideException("Rafın taşınmak istendiği blok/alan sistemde bulunamadı.");
            }

            // İş Kuralı 2: Güncellenen raf kodu (ShelfNumber) aynı bloktaki başka bir rafa ait mi?
            var isShelfExists = await _shelfRepository.AnyAsync(s => s.WarehouseZoneId == updateDto.WarehouseZoneId && s.ShelfNumber.ToLower() == updateDto.ShelfNumber.ToLower() && s.Id != updateDto.Id && s.IsActive);
            if (isShelfExists)
                throw new ClientSideException("Bu bloğun içinde aynı koda sahip başka bir raf zaten mevcut.");

            // İş Kuralı 3: Hacim/Ağırlık düşürülüyorsa, içerideki maldan daha aza indirilemez
            if (updateDto.MaxVolume < shelf.CurrentVolume)
                throw new ClientSideException($"Rafın maksimum hacmi, mevcut dolu hacimden ({shelf.CurrentVolume}) daha küçük olamaz.");

            if (updateDto.MaxWeight < shelf.CurrentWeight)
                throw new ClientSideException($"Rafın maksimum taşıma kapasitesi (Ağırlık), mevcut yükten ({shelf.CurrentWeight}) daha küçük olamaz.");

            // Atamalar
            shelf.ShelfNumber = updateDto.ShelfNumber.Trim();
            shelf.Width = updateDto.Width;
            shelf.Height = updateDto.Height;
            shelf.Depth = updateDto.Depth;
            shelf.MaxVolume = updateDto.MaxVolume;
            shelf.MaxWeight = updateDto.MaxWeight;
            shelf.Status = updateDto.Status;
            shelf.WarehouseZoneId = updateDto.WarehouseZoneId;

            _shelfRepository.Update(shelf);
            await _context.SaveChangesAsync();

            return _mapper.Map<ShelfDto>(shelf);
        }

        /// <summary>
        /// Belirtilen rafı pasif (soft delete) duruma çeker.
        /// </summary>
        public async Task RemoveAsync(Guid id)
        {
            var shelf = await _shelfRepository.Where(s => s.Id == id && s.IsActive).SingleOrDefaultAsync();
            if (shelf == null)
                throw new ClientSideException("Silinmek istenen raf bulunamadı.");

            // İş Kuralı: İçi dolu olan raf silinemez.
            if (shelf.CurrentVolume > 0 || shelf.CurrentWeight > 0)
                throw new ClientSideException("İçerisinde ürün bulunan (doluluk oranı 0'dan büyük) bir raf silinemez. Önce ürünleri transfer ediniz.");

            shelf.IsActive = false; // Soft Delete

            _shelfRepository.Update(shelf);
            await _context.SaveChangesAsync();
        }
    }
}