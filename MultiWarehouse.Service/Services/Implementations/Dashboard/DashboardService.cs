using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MultiWarehouse.Entity.Entities.Definitions;
using MultiWarehouse.Entity.Entities.Inventory;
using MultiWarehouse.Entity.Enums.User;
using MultiWarehouse.Service.Repositories.Interfaces;
using MultiWarehouse.Service.Services.Interfaces.Dashboard;
using MultiWarehouse.Shared.DTOs.DashboardDtos;
using System.Security.Claims;

namespace MultiWarehouse.Service.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        #region Dependencies

        private readonly IGenericRepository<Warehouse> _warehouseRepository;
        private readonly IGenericRepository<Product> _productRepository;
        private readonly IGenericRepository<Stock> _stockRepository;
        private readonly IGenericRepository<StockMovement> _stockMovementRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DashboardService(
            IGenericRepository<Warehouse> warehouseRepository,
            IGenericRepository<Product> productRepository,
            IGenericRepository<Stock> stockRepository,
            IGenericRepository<StockMovement> stockMovementRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _warehouseRepository = warehouseRepository;
            _productRepository = productRepository;
            _stockRepository = stockRepository;
            _stockMovementRepository = stockMovementRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        #endregion

        public async Task<DashboardDto> GetDashboardDataAsync()
        {
            var dashboard = new DashboardDto();
            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);

            var currentUserRole = GetCurrentUserRole();
            var currentWarehouseId = GetCurrentWarehouseId();
            var isSuperAdmin = currentUserRole == UserRole.SuperAdmin.ToString();

            var warehouseQuery = _warehouseRepository.Where(w => w.IsActive);
            if (!isSuperAdmin) warehouseQuery = warehouseQuery.Where(w => w.Id == currentWarehouseId);
            dashboard.TotalWarehouses = await warehouseQuery.CountAsync();

            dashboard.TotalProducts = await _productRepository.Where(p => p.IsActive).CountAsync();

            var stockQuery = _stockRepository.Where(s => s.IsActive);
            if (!isSuperAdmin) stockQuery = stockQuery.Where(s => s.WarehouseId == currentWarehouseId);
            dashboard.TotalActiveStocks = await stockQuery.SumAsync(s => (int?)s.Quantity) ?? 0;

            var movementQuery = _stockMovementRepository.Where(m => m.IsActive);
            if (!isSuperAdmin) movementQuery = movementQuery.Where(m => m.WarehouseId == currentWarehouseId);

            dashboard.DailyMovementsCount = await movementQuery.Where(m => m.CreatedDate >= today).CountAsync();
            dashboard.YesterdayMovementsCount = await movementQuery.Where(m => m.CreatedDate >= yesterday && m.CreatedDate < today).CountAsync();

            if (dashboard.YesterdayMovementsCount == 0)
            {
                dashboard.MovementIncreasePercentage = dashboard.DailyMovementsCount > 0 ? 100 : 0;
            }
            else
            {
                double difference = dashboard.DailyMovementsCount - dashboard.YesterdayMovementsCount;
                dashboard.MovementIncreasePercentage = Math.Round((difference / dashboard.YesterdayMovementsCount) * 100, 1);
            }

            dashboard.WarehouseOccupancies = await warehouseQuery
                .Where(w => w.MaxCapacity > 0)
                .Select(w => new WarehouseOccupancyDto
                {
                    WarehouseId = w.Id,
                    WarehouseName = w.Name,
                    UsedCapacity = w.UsedCapacity,
                    MaxCapacity = w.MaxCapacity,
                    OccupancyRate = Math.Round((w.UsedCapacity / w.MaxCapacity) * 100, 2),
                    Latitude = w.Latitude,
                    Longitude = w.Longitude
                }).ToListAsync();

            dashboard.CriticalStocks = await _productRepository.Where(p => p.IsActive)
                .Select(p => new CriticalStockDto
                {
                    ProductId = p.Id,
                    Sku = p.Sku,
                    ProductName = p.Name,
                    CriticalLevel = p.CriticalLevel,
                    TotalQuantity = p.Stocks
                        .Where(s => s.IsActive && (isSuperAdmin || s.WarehouseId == currentWarehouseId))
                        .Sum(s => s.Quantity)
                })
                .Where(x => x.TotalQuantity <= x.CriticalLevel)
                .OrderBy(x => x.TotalQuantity)
                .Take(10)
                .ToListAsync();

            // 4. SON HAREKETLER (RECENT MOVEMENTS)
            var recentMovements = await movementQuery
                .Include(m => m.Product)
                .Include(m => m.Warehouse)
                .Include(m => m.Shelf)
                .Include(m => m.User)
                .OrderByDescending(m => m.CreatedDate)
                .Take(5)
                .ToListAsync();

            dashboard.RecentMovements = recentMovements.Select(m => new RecentMovementDto
            {
                Id = m.Id,
                MovementType = m.MovementType.ToString(),
                ProductName = m.Product?.Name ?? "Bilinmeyen Ürün",
                Quantity = m.Quantity,
                MovementDate = m.CreatedDate,
                ReferenceNo = string.IsNullOrEmpty(m.Description) ? "Manuel İşlem" : m.Description,
                UserName = m.User != null ? $"{m.User.FirstName} {m.User.LastName}" : "Sistem",
                LocationInfo = m.Warehouse != null && m.Shelf != null
                    ? $"{m.Warehouse.Name} ({m.Shelf.ShelfNumber})"
                    : "Lokal İşlem"
            }).ToList();

            return dashboard;
        }

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