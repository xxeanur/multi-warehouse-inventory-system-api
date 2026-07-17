using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Shared.DTOs.DashboardDtos
{
    /// <summary>
    /// Dashboard ana ekranını besleyen birleştirilmiş (Aggregated) veri modeli.
    /// </summary>
    public class DashboardDto
    {
        // Özet Kartlar (Top Cards)
        public int TotalWarehouses { get; set; }
        public int TotalProducts { get; set; }
        public int TotalActiveStocks { get; set; } // Depolardaki toplam fiziksel ürün adedi
        public int DailyMovementsCount { get; set; } // Bugün yapılan toplam giriş/çıkış/transfer işlemi

        // Grafikler ve Listeler
        public List<WarehouseOccupancyDto> WarehouseOccupancies { get; set; } = new List<WarehouseOccupancyDto>();
        public List<CriticalStockDto> CriticalStocks { get; set; } = new List<CriticalStockDto>();
        public List<RecentMovementDto> RecentMovements { get; set; } = new List<RecentMovementDto>();
    }
}