namespace MultiWarehouse.Shared.DTOs.DashboardDtos
{

    public class DashboardDto
    {
        public int TotalWarehouses { get; set; }
        public int TotalProducts { get; set; }
        public int TotalActiveStocks { get; set; }
        public int DailyMovementsCount { get; set; }

        public int YesterdayMovementsCount { get; set; }
        public double MovementIncreasePercentage { get; set; }

        public List<WarehouseOccupancyDto> WarehouseOccupancies { get; set; } = new List<WarehouseOccupancyDto>();
        public List<CriticalStockDto> CriticalStocks { get; set; } = new List<CriticalStockDto>();
        public List<RecentMovementDto> RecentMovements { get; set; } = new List<RecentMovementDto>();
    }
}