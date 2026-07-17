using Microsoft.EntityFrameworkCore;
using MultiWarehouse.Entity.Entities;
using MultiWarehouse.Service.Context;
using MultiWarehouse.Service.Services.Interfaces;
using MultiWarehouse.Shared.DTOs.DashboardDtos;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MultiWarehouse.Service.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardDto> GetDashboardDataAsync()
        {
            var dashboard = new DashboardDto();
            var today = DateTime.UtcNow.Date;

            // 1. ÖZET KARTLAR (Top Cards)
            dashboard.TotalWarehouses = await _context.Set<Warehouse>().CountAsync(w => w.IsActive);
            dashboard.TotalProducts = await _context.Set<Product>().CountAsync(p => p.IsActive);

            // Tüm depolardaki toplam stok miktarının toplamı (Miktar null gelebileceği için kontrol ekliyoruz)
            dashboard.TotalActiveStocks = await _context.Set<Stock>()
                .Where(s => s.IsActive)
                .SumAsync(s => s.Quantity);

            // Sadece bugün gerçekleşen hareketlerin sayısı
            dashboard.DailyMovementsCount = await _context.Set<StockMovement>()
                .CountAsync(m => m.IsActive && m.MovementDate >= today);

            // 2. DEPO DOLULUK ORANLARI (Grafik İçin)
            // Sadece kapasitesi 0'dan büyük olanları alıyoruz ki sıfıra bölme (DivideByZero) hatası yemeyelim.
            var warehouses = await _context.Set<Warehouse>()
                .Where(w => w.IsActive && w.MaxCapacity > 0)
                .Select(w => new WarehouseOccupancyDto
                {
                    WarehouseName = w.Name,
                    UsedCapacity = w.UsedCapacity,
                    MaxCapacity = w.MaxCapacity,
                    // Yüzde hesaplama: (Kullanılan / Maksimum) * 100
                    OccupancyRate = Math.Round((w.UsedCapacity / w.MaxCapacity) * 100, 2)
                }).ToListAsync();

            dashboard.WarehouseOccupancies = warehouses;

            // 3. KRİTİK STOK ALARMI (Bitmek üzere olan ürünler)
            // Önce her ürünün toplam stok miktarını buluyoruz, sonra kendi kritik seviyesi ile kıyaslıyoruz.
            var criticalStocksQuery = await _context.Set<Product>()
                .Where(p => p.IsActive)
                .Select(p => new CriticalStockDto
                {
                    ProductId = p.Id,
                    Sku = p.Sku,
                    ProductName = p.Name,
                    CriticalLevel = p.CriticalLevel,
                    TotalQuantity = p.Stocks.Where(s => s.IsActive).Sum(s => s.Quantity)
                })
                .Where(x => x.TotalQuantity <= x.CriticalLevel) // Sadece kritik seviyeye inenleri veya bitenleri al
                .OrderBy(x => x.TotalQuantity) // En az kalanı en üste koy
                .Take(10) // Ekrana sadece en acil 10 tanesini gönder
                .ToListAsync();

            dashboard.CriticalStocks = criticalStocksQuery;

            // 4. SON STOK HAREKETLERİ (Geçmiş loglar)
            dashboard.RecentMovements = await _context.Set<StockMovement>()
                .Include(m => m.Product)
                .Include(m => m.User)
                .Where(m => m.IsActive)
                .OrderByDescending(m => m.MovementDate)
                .Take(10) // Sadece en son gerçekleşen 10 hareketi gönder
                .Select(m => new RecentMovementDto
                {
                    MovementType = m.MovementType.ToString(), // Enum'u string'e çeviriyoruz
                    ProductName = m.Product.Name,
                    Quantity = m.Quantity,
                    MovementDate = m.MovementDate,
                    ReferenceNo = m.ReferenceNo,
                    UserName = m.User.FirstName + " " + m.User.LastName
                }).ToListAsync();

            return dashboard;
        }
    }
}