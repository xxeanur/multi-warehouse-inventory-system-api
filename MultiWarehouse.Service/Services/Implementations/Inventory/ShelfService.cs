using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MultiWarehouse.Entity.Entities.Definitions;
using MultiWarehouse.Entity.Enums.User;
using MultiWarehouse.Service.Exceptions;
using MultiWarehouse.Service.Repositories.Interfaces;
using MultiWarehouse.Service.Services.Interfaces.Inventory;
using MultiWarehouse.Shared.DTOs.ShelfDtos;
using MultiWarehouse.Shared.Pagination;
using System.Security.Claims;

namespace MultiWarehouse.Service.Services.Implementations.Inventory
{
    public class ShelfService : IShelfService
    {
        private readonly IGenericRepository<Shelf> _shelfRepository;
        private readonly IGenericRepository<WarehouseZone> _zoneRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ShelfService(
            IGenericRepository<Shelf> shelfRepository,
            IGenericRepository<WarehouseZone> zoneRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _shelfRepository = shelfRepository;
            _zoneRepository = zoneRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        #region Write Operations

        public async Task<ShelfDto> CreateAsync(ShelfCreateDto createDto)
        {
            var zone = await _zoneRepository.Where(z => z.Id == createDto.WarehouseZoneId && z.IsActive).SingleOrDefaultAsync();
            if (zone == null)
                throw new ClientSideException("Raf eklenmek istenen blok/alan sistemde bulunamadı.");

            var currentUserRole = GetCurrentUserRole();
            if (currentUserRole != UserRole.SuperAdmin.ToString())
            {
                var currentWarehouseId = GetCurrentWarehouseId();
                if (zone.WarehouseId != currentWarehouseId)
                    throw new ClientSideException("Sadece yetkili olduğunuz deponun alanlarına raf ekleyebilirsiniz.");
            }

            var isShelfExists = await _shelfRepository.AnyAsync(s => s.WarehouseZoneId == createDto.WarehouseZoneId && s.ShelfNumber.ToLower() == createDto.ShelfNumber.ToLower() && s.IsActive);
            if (isShelfExists)
                throw new ClientSideException("Bu bloğun içinde aynı koda sahip bir raf zaten mevcut.");

            var shelf = _mapper.Map<Shelf>(createDto);


            shelf.MaxVolume = Math.Round(createDto.Width * createDto.Height * createDto.Depth, 2);
            shelf.CurrentVolume = 0;
            shelf.CurrentWeight = 0;

            await _shelfRepository.AddAsync(shelf);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ShelfDto>(shelf);
        }

        public async Task<ShelfDto> UpdateAsync(ShelfUpdateDto updateDto)
        {
            var shelf = await _shelfRepository.Where(s => s.Id == updateDto.Id && s.IsActive).SingleOrDefaultAsync();
            if (shelf == null)
                throw new ClientSideException("Güncellenmek istenen raf bulunamadı.");

            var currentZone = await _zoneRepository.Where(z => z.Id == shelf.WarehouseZoneId).SingleOrDefaultAsync();
            var currentUserRole = GetCurrentUserRole();
            var currentWarehouseId = GetCurrentWarehouseId();

            if (currentUserRole != UserRole.SuperAdmin.ToString() && currentZone?.WarehouseId != currentWarehouseId)
                throw new ClientSideException("Sadece yetkili olduğunuz depodaki rafları güncelleyebilirsiniz.");

            if (shelf.WarehouseZoneId != updateDto.WarehouseZoneId)
            {
                var newZone = await _zoneRepository.Where(z => z.Id == updateDto.WarehouseZoneId && z.IsActive).SingleOrDefaultAsync();
                if (newZone == null)
                    throw new ClientSideException("Rafın taşınmak istendiği blok/alan sistemde bulunamadı.");

                if (currentUserRole != UserRole.SuperAdmin.ToString() && newZone.WarehouseId != currentWarehouseId)
                    throw new ClientSideException("Rafı başka bir depoya taşıyamazsınız.");
            }

            var isShelfExists = await _shelfRepository.AnyAsync(s => s.WarehouseZoneId == updateDto.WarehouseZoneId && s.ShelfNumber.ToLower() == updateDto.ShelfNumber.ToLower() && s.Id != updateDto.Id && s.IsActive);
            if (isShelfExists)
                throw new ClientSideException("Bu bloğun içinde aynı koda sahip başka bir raf zaten mevcut.");

            double newCalculatedVolume = Math.Round(updateDto.Width * updateDto.Height * updateDto.Depth, 2);
            double roundedMaxWeight = Math.Round(updateDto.MaxWeight, 2);

            if (newCalculatedVolume < shelf.CurrentVolume)
                throw new ClientSideException($"Rafın maksimum hacmi, mevcut dolu hacimden ({shelf.CurrentVolume}) daha küçük olamaz.");

            if (roundedMaxWeight < shelf.CurrentWeight)
                throw new ClientSideException($"Rafın maksimum taşıma kapasitesi (Ağırlık), mevcut yükten ({shelf.CurrentWeight}) daha küçük olamaz.");

            shelf.ShelfNumber = updateDto.ShelfNumber.Trim();
            shelf.Width = updateDto.Width;
            shelf.Height = updateDto.Height;
            shelf.Depth = updateDto.Depth;
            shelf.MaxVolume = newCalculatedVolume;
            shelf.MaxWeight = roundedMaxWeight;
            shelf.Status = updateDto.Status;
            shelf.WarehouseZoneId = updateDto.WarehouseZoneId;

            _shelfRepository.Update(shelf);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ShelfDto>(shelf);
        }

        public async Task RemoveAsync(Guid id)
        {
            var shelf = await _shelfRepository.Where(s => s.Id == id && s.IsActive).SingleOrDefaultAsync();
            if (shelf == null)
                throw new ClientSideException("Silinmek istenen raf bulunamadı.");

            var currentUserRole = GetCurrentUserRole();
            if (currentUserRole != UserRole.SuperAdmin.ToString())
            {
                var currentWarehouseId = GetCurrentWarehouseId();
                var zone = await _zoneRepository.Where(z => z.Id == shelf.WarehouseZoneId).SingleOrDefaultAsync();
                if (zone != null && zone.WarehouseId != currentWarehouseId)
                    throw new ClientSideException("Sadece yetkili olduğunuz depodaki rafları silebilirsiniz.");
            }

            if (shelf.CurrentVolume > 0 || shelf.CurrentWeight > 0)
                throw new ClientSideException("İçerisinde ürün bulunan (doluluk oranı 0'dan büyük) bir raf silinemez. Önce ürünleri transfer ediniz.");

            shelf.IsActive = false;

            _shelfRepository.Update(shelf);
            await _unitOfWork.SaveChangesAsync();
        }

        #endregion

        #region Read Operations

        public async Task<ShelfDto> GetByIdAsync(Guid id)
        {
            var shelf = await _shelfRepository.Where(s => s.Id == id && s.IsActive).SingleOrDefaultAsync();
            if (shelf == null)
                throw new ClientSideException("Raf bulunamadı.");

            var currentUserRole = GetCurrentUserRole();
            if (currentUserRole != UserRole.SuperAdmin.ToString())
            {
                var zone = await _zoneRepository.Where(z => z.Id == shelf.WarehouseZoneId).SingleOrDefaultAsync();
                var currentWarehouseId = GetCurrentWarehouseId();
                if (zone != null && zone.WarehouseId != currentWarehouseId)
                    throw new ClientSideException("Başka bir depoya ait rafı görüntüleme yetkiniz yok.");
            }

            return _mapper.Map<ShelfDto>(shelf);
        }

        public async Task<IEnumerable<ShelfDto>> GetAllAsync()
        {
            IQueryable<Shelf> query = _shelfRepository.Where(s => s.IsActive);

            var currentUserRole = GetCurrentUserRole();
            if (currentUserRole != UserRole.SuperAdmin.ToString())
            {
                var currentWarehouseId = GetCurrentWarehouseId();
                var allowedZoneIds = await _zoneRepository
                    .Where(z => z.WarehouseId == currentWarehouseId && z.IsActive)
                    .Select(z => z.Id)
                    .ToListAsync();

                query = query.Where(s => allowedZoneIds.Contains(s.WarehouseZoneId));
            }

            var shelves = await query.ToListAsync();
            return _mapper.Map<IEnumerable<ShelfDto>>(shelves);
        }

        public async Task<PagedResult<ShelfDto>> GetPagedAsync(PaginationParams paginationParams)
        {
            var currentUserRole = GetCurrentUserRole();
            List<Guid> allowedZoneIds = new List<Guid>();

            if (currentUserRole != UserRole.SuperAdmin.ToString())
            {
                var currentWarehouseId = GetCurrentWarehouseId();
                allowedZoneIds = await _zoneRepository
                    .Where(z => z.WarehouseId == currentWarehouseId && z.IsActive)
                    .Select(z => z.Id)
                    .ToListAsync();
            }

            var pagedEntities = await _shelfRepository.GetPagedAsync(
                paginationParams,
                filter: s => s.IsActive && (currentUserRole == UserRole.SuperAdmin.ToString() || allowedZoneIds.Contains(s.WarehouseZoneId))
            );

            return _mapper.Map<PagedResult<ShelfDto>>(pagedEntities);
        }

        public async Task<IEnumerable<ShelfDto>> GetAllByZoneIdAsync(Guid zoneId)
        {
            await ValidateZoneAccessAsync(zoneId);

            var shelves = await _shelfRepository.Where(s => s.WarehouseZoneId == zoneId && s.IsActive).ToListAsync();
            return _mapper.Map<IEnumerable<ShelfDto>>(shelves);
        }

        public async Task<PagedResult<ShelfDto>> GetPagedByZoneIdAsync(PaginationParams paginationParams, Guid zoneId)
        {
            await ValidateZoneAccessAsync(zoneId);

            var pagedEntities = await _shelfRepository.GetPagedAsync(
                paginationParams,
                filter: s => s.IsActive && s.WarehouseZoneId == zoneId
            );

            return _mapper.Map<PagedResult<ShelfDto>>(pagedEntities);
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


        private async Task ValidateZoneAccessAsync(Guid zoneId)
        {
            var currentUserRole = GetCurrentUserRole();
            if (currentUserRole != UserRole.SuperAdmin.ToString())
            {
                var zone = await _zoneRepository.Where(z => z.Id == zoneId).SingleOrDefaultAsync();
                var currentWarehouseId = GetCurrentWarehouseId();
                if (zone != null && zone.WarehouseId != currentWarehouseId)
                    throw new ClientSideException("Başka bir depoya ait alanın (Zone) verilerini çekemezsiniz.");
            }
        }

        #endregion
    }
}