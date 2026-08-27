using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MultiWarehouse.Entity.Entities.Definitions;
using MultiWarehouse.Entity.Enums.User;
using MultiWarehouse.Service.Exceptions;
using MultiWarehouse.Service.Repositories.Interfaces;
using MultiWarehouse.Service.Services.Interfaces.Definations;
using MultiWarehouse.Shared.DTOs.WarehouseZoneDtos;
using System.Security.Claims;

namespace MultiWarehouse.Service.Services.Implementations.Definitions
{
    public class WarehouseZoneService : IWarehouseZoneService
    {
        private readonly IGenericRepository<WarehouseZone> _zoneRepository;
        private readonly IGenericRepository<Warehouse> _warehouseRepository;
        private readonly IGenericRepository<Shelf> _shelfRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WarehouseZoneService(
            IGenericRepository<WarehouseZone> zoneRepository,
            IGenericRepository<Warehouse> warehouseRepository,
            IGenericRepository<Shelf> shelfRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _zoneRepository = zoneRepository;
            _warehouseRepository = warehouseRepository;
            _shelfRepository = shelfRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        #region Write Operations

        public async Task<WarehouseZoneDto> CreateAsync(WarehouseZoneCreateDto createDto)
        {
            createDto.ZoneName = createDto.ZoneName.Trim();

            var currentUserRole = GetCurrentUserRole();
            if (currentUserRole != UserRole.SuperAdmin.ToString())
            {
                var currentWarehouseId = GetCurrentWarehouseId();
                if (currentWarehouseId != createDto.WarehouseId)
                    throw new ClientSideException("Sadece yetkili olduğunuz depoya yeni bir alan ekleyebilirsiniz.");
            }

            var isWarehouseExists = await _warehouseRepository.AnyAsync(x => x.Id == createDto.WarehouseId && x.IsActive);
            if (!isWarehouseExists)
                throw new ClientSideException("Blok eklenmek istenen depo sistemde bulunamadı.");

            var isZoneNameExists = await _zoneRepository.AnyAsync(x =>
                x.WarehouseId == createDto.WarehouseId &&
                x.ZoneName.ToLower() == createDto.ZoneName.ToLower() &&
                x.IsActive);

            if (isZoneNameExists)
                throw new ClientSideException("Bu deponun içinde aynı isimde bir blok zaten mevcut.");

            var zone = _mapper.Map<WarehouseZone>(createDto);

            await _zoneRepository.AddAsync(zone);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<WarehouseZoneDto>(zone);
        }

        public async Task<WarehouseZoneDto> UpdateAsync(WarehouseZoneUpdateDto updateDto)
        {
            updateDto.ZoneName = updateDto.ZoneName.Trim();

            var zone = await _zoneRepository.Where(x => x.Id == updateDto.Id && x.IsActive).SingleOrDefaultAsync();
            if (zone == null)
                throw new ClientSideException("Güncellenmek istenen depo alanı bulunamadı.");

            var currentUserRole = GetCurrentUserRole();
            if (currentUserRole != UserRole.SuperAdmin.ToString())
            {
                var currentWarehouseId = GetCurrentWarehouseId();
                if (currentWarehouseId != zone.WarehouseId || currentWarehouseId != updateDto.WarehouseId)
                    throw new ClientSideException("Sadece yetkili olduğunuz depodaki alanları güncelleyebilirsiniz.");
            }

            var isWarehouseExists = await _warehouseRepository.AnyAsync(x => x.Id == updateDto.WarehouseId && x.IsActive);
            if (!isWarehouseExists)
                throw new ClientSideException("Seçilen depo sistemde bulunamadı.");

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
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<WarehouseZoneDto>(zone);
        }

        public async Task RemoveAsync(Guid id)
        {
            var zone = await _zoneRepository.Where(x => x.Id == id && x.IsActive).SingleOrDefaultAsync();
            if (zone == null)
                throw new ClientSideException("Silinmek istenen depo alanı bulunamadı.");

            var currentUserRole = GetCurrentUserRole();
            if (currentUserRole != UserRole.SuperAdmin.ToString())
            {
                var currentWarehouseId = GetCurrentWarehouseId();
                if (currentWarehouseId != zone.WarehouseId)
                    throw new ClientSideException("Sadece yetkili olduğunuz depodaki alanları silebilirsiniz.");
            }

            // İş Kuralı: İçinde aktif raf bulunan bir alan (Zone) silinemez.
            var hasActiveShelves = await _shelfRepository.AnyAsync(s => s.WarehouseZoneId == id && s.IsActive);
            if (hasActiveShelves)
                throw new ClientSideException("Bu depo alanı silinemez! İçerisinde aktif raflar bulunmaktadır. Lütfen önce rafları silin veya başka bir alana taşıyın.");

            zone.IsActive = false;

            _zoneRepository.Update(zone);
            await _unitOfWork.SaveChangesAsync();
        }

        #endregion

        #region Read Operations

        public async Task<WarehouseZoneDto> GetByIdAsync(Guid id)
        {
            var zone = await _zoneRepository.Where(x => x.Id == id && x.IsActive).SingleOrDefaultAsync();
            if (zone == null)
                throw new ClientSideException("Depo alanı bulunamadı.");

            var currentUserRole = GetCurrentUserRole();
            if (currentUserRole != UserRole.SuperAdmin.ToString())
            {
                var currentWarehouseId = GetCurrentWarehouseId();
                if (currentWarehouseId != zone.WarehouseId)
                    throw new ClientSideException("Başka bir depoya ait alanı görüntüleme yetkiniz yok.");
            }

            return _mapper.Map<WarehouseZoneDto>(zone);
        }

        public async Task<IEnumerable<WarehouseZoneDto>> GetAllAsync()
        {
            IQueryable<WarehouseZone> query = _zoneRepository.Where(x => x.IsActive);

            var currentUserRole = GetCurrentUserRole();
            if (currentUserRole != UserRole.SuperAdmin.ToString())
            {
                var currentWarehouseId = GetCurrentWarehouseId();
                query = query.Where(x => x.WarehouseId == currentWarehouseId);
            }

            var zones = await query.ToListAsync();
            return _mapper.Map<IEnumerable<WarehouseZoneDto>>(zones);
        }

        public async Task<IEnumerable<WarehouseZoneDto>> GetAllByWarehouseIdAsync(Guid warehouseId)
        {
            var currentUserRole = GetCurrentUserRole();
            if (currentUserRole != UserRole.SuperAdmin.ToString())
            {
                var currentWarehouseId = GetCurrentWarehouseId();
                if (currentWarehouseId != warehouseId)
                    throw new ClientSideException("Başka bir depoya ait alanları görüntüleme yetkiniz yok.");
            }

            var zones = await _zoneRepository.Where(x => x.WarehouseId == warehouseId && x.IsActive).ToListAsync();
            return _mapper.Map<IEnumerable<WarehouseZoneDto>>(zones);
        }

        #endregion

        #region Private Helpers

        private string GetCurrentUserRole()
        {
            return _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        private Guid? GetCurrentWarehouseId()
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirst("WarehouseId");
            if (claim != null && Guid.TryParse(claim.Value, out var warehouseId))
                return warehouseId;

            return null;
        }

        #endregion
    }
}