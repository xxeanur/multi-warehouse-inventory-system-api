namespace MultiWarehouse.Shared.DTOs.DashboardDtos
{
    public class WarehouseOccupancyDto
    {
        public Guid WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public double UsedCapacity { get; set; }
        public double MaxCapacity { get; set; }
        public double OccupancyRate { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}