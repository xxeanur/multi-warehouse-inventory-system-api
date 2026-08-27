using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MultiWarehouse.Entity.Entities.Inventory;
using MultiWarehouse.Entity.Enums.User;
using MultiWarehouse.Service.Exceptions;
using MultiWarehouse.Service.Repositories.Interfaces;
using MultiWarehouse.Service.Services.Interfaces.Inventory;
using MultiWarehouse.Shared.DTOs.StockDtos;
using MultiWarehouse.Shared.Pagination;
using System.Security.Claims;

namespace MultiWarehouse.Service.Services.Implementations.Inventory
{

    public class StockService : IStockService
    {
        #region Dependencies

        private readonly IGenericRepository<Stock> _stockRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public StockService(
            IGenericRepository<Stock> stockRepository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _stockRepository = stockRepository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region Read Operations (Non-Paged)

        public async Task<StockDto> GetByIdAsync(Guid id)
        {
            var stock = await _stockRepository
                .Where(s => s.Id == id && s.IsActive)
                .Select(s => new StockDto
                {
                    Id = s.Id,
                    ProductId = s.ProductId,
                    ProductName = s.Product != null ? s.Product.Name : "-",
                    ProductCode = s.Product != null ? s.Product.Sku : "-",
                    WarehouseId = s.WarehouseId,
                    WarehouseName = s.Warehouse != null ? s.Warehouse.Name : "-",
                    ShelfId = s.ShelfId,
                    ShelfCode = s.Shelf != null ? s.Shelf.ShelfNumber : "-",
                    Quantity = s.Quantity,
                    ReservedQuantity = s.ReservedQuantity,
                    CreatedDate = s.CreatedDate,
                    IsActive = s.IsActive
                })
                .SingleOrDefaultAsync();

            if (stock == null) throw new ClientSideException("Stok kaydı bulunamadı.");

            var currentUserRole = GetCurrentUserRole();
            if (currentUserRole != UserRole.SuperAdmin.ToString())
            {
                var currentWarehouseId = GetCurrentWarehouseId();
                if (currentWarehouseId != stock.WarehouseId)
                    throw new ClientSideException("Başka bir depoya ait stoğu görüntüleme yetkiniz yok.");
            }

            return stock;
        }

        public async Task<IEnumerable<StockDto>> GetAllAsync()
        {
            return await GetStocksByConditionAsync(s => s.IsActive && s.Quantity > 0);
        }

        public async Task<IEnumerable<StockDto>> GetAllByProductIdAsync(Guid productId)
        {
            return await GetStocksByConditionAsync(s => s.ProductId == productId && s.IsActive && s.Quantity > 0);
        }

        public async Task<IEnumerable<StockDto>> GetAllByWarehouseIdAsync(Guid warehouseId)
        {
            var currentUserRole = GetCurrentUserRole();
            if (currentUserRole != UserRole.SuperAdmin.ToString())
            {
                var currentWarehouseId = GetCurrentWarehouseId();
                if (currentWarehouseId != warehouseId)
                    throw new ClientSideException("Başka bir deponun stoklarını listeleyemezsiniz.");
            }

            return await GetStocksByConditionAsync(s => s.WarehouseId == warehouseId && s.IsActive && s.Quantity > 0);
        }

        public async Task<IEnumerable<StockDto>> GetAllByShelfIdAsync(Guid shelfId)
        {
            return await GetStocksByConditionAsync(s => s.ShelfId == shelfId && s.IsActive && s.Quantity > 0);
        }

        #endregion

        #region Read Operations (Paged & Optimized)

        public async Task<PagedResult<StockDto>> GetPagedAsync(PaginationParams paginationParams)
        {
            return await GetPagedStocksByConditionAsync(s => s.IsActive && s.Quantity > 0, paginationParams);
        }

        public async Task<PagedResult<StockDto>> GetPagedByProductIdAsync(PaginationParams paginationParams, Guid productId)
        {
            return await GetPagedStocksByConditionAsync(s => s.IsActive && s.ProductId == productId && s.Quantity > 0, paginationParams);
        }

        public async Task<PagedResult<StockDto>> GetPagedByWarehouseIdAsync(PaginationParams paginationParams, Guid warehouseId)
        {
            var currentUserRole = GetCurrentUserRole();
            if (currentUserRole != UserRole.SuperAdmin.ToString())
            {
                var currentWarehouseId = GetCurrentWarehouseId();
                if (currentWarehouseId != warehouseId)
                    throw new ClientSideException("Başka bir deponun stoklarını listeleyemezsiniz.");
            }

            return await GetPagedStocksByConditionAsync(s => s.IsActive && s.WarehouseId == warehouseId && s.Quantity > 0, paginationParams);
        }

        public async Task<PagedResult<StockDto>> GetPagedByShelfIdAsync(PaginationParams paginationParams, Guid shelfId)
        {
            return await GetPagedStocksByConditionAsync(s => s.IsActive && s.ShelfId == shelfId && s.Quantity > 0, paginationParams);
        }

        #endregion

        #region Private Helpers

        private async Task<IEnumerable<StockDto>> GetStocksByConditionAsync(System.Linq.Expressions.Expression<Func<Stock, bool>> condition)
        {
            var query = _stockRepository.Where(condition);
            var currentUserRole = GetCurrentUserRole();


            if (currentUserRole != UserRole.SuperAdmin.ToString())
            {
                var currentWarehouseId = GetCurrentWarehouseId();
                query = query.Where(s => s.WarehouseId == currentWarehouseId);
            }

            return await query
                .OrderByDescending(s => s.CreatedDate)
                .Select(s => new StockDto
                {
                    Id = s.Id,
                    ProductId = s.ProductId,
                    ProductName = s.Product != null ? s.Product.Name : "-",
                    ProductCode = s.Product != null ? s.Product.Sku : "-",
                    WarehouseId = s.WarehouseId,
                    WarehouseName = s.Warehouse != null ? s.Warehouse.Name : "-",
                    ShelfId = s.ShelfId,
                    ShelfCode = s.Shelf != null ? s.Shelf.ShelfNumber : "-",
                    Quantity = s.Quantity,
                    ReservedQuantity = s.ReservedQuantity,
                    CreatedDate = s.CreatedDate,
                    IsActive = s.IsActive
                }).ToListAsync();
        }

        private async Task<PagedResult<StockDto>> GetPagedStocksByConditionAsync(
            System.Linq.Expressions.Expression<Func<Stock, bool>> condition,
            PaginationParams paginationParams)
        {
            var query = _stockRepository.Where(condition);
            var currentUserRole = GetCurrentUserRole();

            if (currentUserRole != UserRole.SuperAdmin.ToString())
            {
                var currentWarehouseId = GetCurrentWarehouseId();
                query = query.Where(s => s.WarehouseId == currentWarehouseId);
            }

            var totalCount = await query.CountAsync();

            var data = await query
                .OrderByDescending(s => s.CreatedDate)
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .Select(s => new StockDto
                {
                    Id = s.Id,
                    ProductId = s.ProductId,
                    ProductName = s.Product != null ? s.Product.Name : "-",
                    ProductCode = s.Product != null ? s.Product.Sku : "-",
                    WarehouseId = s.WarehouseId,
                    WarehouseName = s.Warehouse != null ? s.Warehouse.Name : "-",
                    ShelfId = s.ShelfId,
                    ShelfCode = s.Shelf != null ? s.Shelf.ShelfNumber : "-",
                    Quantity = s.Quantity,
                    ReservedQuantity = s.ReservedQuantity,
                    CreatedDate = s.CreatedDate,
                    IsActive = s.IsActive
                }).ToListAsync();

            return new PagedResult<StockDto>(data, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
        }

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