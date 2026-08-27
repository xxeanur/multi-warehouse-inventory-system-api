using MultiWarehouse.Entity.Enums.Warehouse;

namespace MultiWarehouse.Shared.DTOs.WarehouseDtos
{
    public class WarehouseCreateDto
    {
        public string Name { get; set; } = string.Empty;

        public string Country { get; set; } = "Türkiye";
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string FullAddress { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Phone { get; set; } = string.Empty;
        public Guid? ManagerId { get; set; }
        public double MaxCapacity { get; set; }

        public WarehouseOperationalStatus OperationalStatus { get; set; }
    }
}