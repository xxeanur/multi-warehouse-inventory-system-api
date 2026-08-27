using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MultiWarehouse.Entity.Entities.Definitions;
using MultiWarehouse.Entity.Entities.Identity;
using MultiWarehouse.Entity.Enums.User;
using MultiWarehouse.Service.Exceptions;
using MultiWarehouse.Service.Repositories.Interfaces;
using MultiWarehouse.Service.Services.Interfaces.Definations;
using MultiWarehouse.Shared.DTOs.WarehouseDtos;
using MultiWarehouse.Shared.Pagination;
using System.Security.Claims;

namespace MultiWarehouse.Service.Services.Implementations.Definitions
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IGenericRepository<Warehouse> _warehouseRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WarehouseService(
            IGenericRepository<Warehouse> warehouseRepository,
            IGenericRepository<User> userRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _warehouseRepository = warehouseRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        #region Write Operations (SuperAdmin Only - Controller Protected)

        public async Task<WarehouseDto> CreateAsync(WarehouseCreateDto createDto)
        {
            var isNameExists = await _warehouseRepository.AnyAsync(x => x.Name.ToLower() == createDto.Name.ToLower() && x.IsActive);
            if (isNameExists)
                throw new ClientSideException("Bu isimde bir depo zaten mevcut.");

            if (createDto.ManagerId.HasValue)
            {
                var managerUser = await _userRepository.Where(u => u.Id == createDto.ManagerId.Value && u.IsActive).SingleOrDefaultAsync();
                if (managerUser == null)
                    throw new ClientSideException("Seçilen yönetici sistemde bulunamadı veya pasif durumda.");

                if (managerUser.Role != UserRole.WarehouseManager)
                    throw new ClientSideException("Seçilen kullanıcının rolü 'Depo Sorumlusu (WarehouseManager)' olmalıdır.");
            }

            var warehouse = _mapper.Map<Warehouse>(createDto);
            warehouse.UsedCapacity = 0;

            await _warehouseRepository.AddAsync(warehouse);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<WarehouseDto>(warehouse);
        }

        public async Task<WarehouseDto> UpdateAsync(WarehouseUpdateDto updateDto)
        {
            var warehouse = await _warehouseRepository.Where(x => x.Id == updateDto.Id && x.IsActive).SingleOrDefaultAsync();
            if (warehouse == null)
                throw new ClientSideException("Güncellenmek istenen depo bulunamadı.");

            var isNameExists = await _warehouseRepository.AnyAsync(x => x.Name.ToLower() == updateDto.Name.ToLower() && x.Id != updateDto.Id && x.IsActive);
            if (isNameExists)
                throw new ClientSideException("Bu depo adı sistemde başka bir depo tarafından kullanılıyor.");

            if (updateDto.ManagerId.HasValue && updateDto.ManagerId != warehouse.ManagerId)
            {
                var managerUser = await _userRepository.Where(u => u.Id == updateDto.ManagerId.Value && u.IsActive).SingleOrDefaultAsync();
                if (managerUser == null)
                    throw new ClientSideException("Seçilen yönetici sistemde bulunamadı veya pasif durumda.");

                if (managerUser.Role != UserRole.WarehouseManager)
                    throw new ClientSideException("Seçilen kullanıcının rolü 'Depo Sorumlusu (WarehouseManager)' olmalıdır.");
            }

            warehouse.Name = updateDto.Name;
            warehouse.Country = updateDto.Country;
            warehouse.City = updateDto.City;
            warehouse.District = updateDto.District;
            warehouse.FullAddress = updateDto.FullAddress;
            warehouse.Latitude = updateDto.Latitude;
            warehouse.Longitude = updateDto.Longitude;
            warehouse.Phone = updateDto.Phone;
            warehouse.ManagerId = updateDto.ManagerId;
            warehouse.OperationalStatus = updateDto.OperationalStatus;

            if (updateDto.MaxCapacity < warehouse.UsedCapacity)
                throw new ClientSideException($"Deponun maksimum kapasitesi, mevcut doluluk oranından ({warehouse.UsedCapacity}) küçük olamaz.");

            warehouse.MaxCapacity = updateDto.MaxCapacity;

            _warehouseRepository.Update(warehouse);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<WarehouseDto>(warehouse);
        }

        public async Task RemoveAsync(Guid id)
        {
            var warehouse = await _warehouseRepository.Where(x => x.Id == id && x.IsActive).SingleOrDefaultAsync();
            if (warehouse == null)
                throw new ClientSideException("Silinmek istenen depo bulunamadı.");

            if (warehouse.UsedCapacity > 0)
                throw new ClientSideException("İçerisinde ürün bulunan bir depo silinemez. Önce stokları transfer etmelisiniz.");

            warehouse.IsActive = false;

            _warehouseRepository.Update(warehouse);
            await _unitOfWork.SaveChangesAsync();
        }

        #endregion

        #region Read Operations (Row-Level Security Applied)

        public async Task<WarehouseDto> GetByIdAsync(Guid id)
        {
            var currentUserRole = GetCurrentUserRole();
            if (currentUserRole != UserRole.SuperAdmin.ToString())
            {
                var currentWarehouseId = await GetCurrentWarehouseIdAsync();
                if (currentWarehouseId != id)
                    throw new ClientSideException("Başka bir deponun bilgilerini görüntüleme yetkiniz yok.");
            }

            var warehouse = await _warehouseRepository.Where(x => x.Id == id && x.IsActive).SingleOrDefaultAsync();
            if (warehouse == null)
                throw new ClientSideException("Depo bulunamadı.");

            var dto = _mapper.Map<WarehouseDto>(warehouse);

            if (warehouse.ManagerId.HasValue)
            {
                var manager = await _userRepository.Where(u => u.Id == warehouse.ManagerId.Value).SingleOrDefaultAsync();
                dto.ManagerName = manager != null ? $"{manager.FirstName} {manager.LastName}" : "Bilinmeyen Yönetici";
            }

            return dto;
        }

        public async Task<IEnumerable<WarehouseDto>> GetAllAsync()
        {
            IQueryable<Warehouse> query = _warehouseRepository.Where(x => x.IsActive);

            var currentUserRole = GetCurrentUserRole();
            if (currentUserRole != UserRole.SuperAdmin.ToString())
            {
                var currentWarehouseId = await GetCurrentWarehouseIdAsync();
                query = query.Where(x => x.Id == currentWarehouseId);
            }

            var warehouses = await query.ToListAsync();
            var dtos = _mapper.Map<IEnumerable<WarehouseDto>>(warehouses).ToList();

            var managerIds = warehouses.Where(w => w.ManagerId.HasValue).Select(w => w.ManagerId.Value).Distinct().ToList();
            if (managerIds.Any())
            {
                var managers = await _userRepository.Where(u => managerIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => $"{u.FirstName} {u.LastName}");
                foreach (var dto in dtos)
                {
                    if (dto.ManagerId.HasValue && managers.ContainsKey(dto.ManagerId.Value))
                    {
                        dto.ManagerName = managers[dto.ManagerId.Value];
                    }
                }
            }

            return dtos;
        }

        public async Task<PagedResult<WarehouseDto>> GetPagedAsync(PaginationParams paginationParams)
        {
            var currentUserRole = GetCurrentUserRole();
            Guid? currentWarehouseId = null;

            if (currentUserRole != UserRole.SuperAdmin.ToString())
            {
                currentWarehouseId = await GetCurrentWarehouseIdAsync();
            }

            var pagedEntities = await _warehouseRepository.GetPagedAsync(
                paginationParams,
                filter: w => w.IsActive && (currentUserRole == UserRole.SuperAdmin.ToString() || w.Id == currentWarehouseId)
            );


            return _mapper.Map<PagedResult<WarehouseDto>>(pagedEntities);
        }

        #endregion

        #region Private Helpers

        private string GetCurrentUserRole()
        {
            return _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        private async Task<Guid?> GetCurrentWarehouseIdAsync()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim != null && Guid.TryParse(userIdClaim, out var userId))
            {
                var user = await _userRepository.Where(u => u.Id == userId && u.IsActive).SingleOrDefaultAsync();
                return user?.WarehouseId;
            }

            return null;
        }

        #endregion
    }
}