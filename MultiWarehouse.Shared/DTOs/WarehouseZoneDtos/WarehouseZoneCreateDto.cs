using MultiWarehouse.Entity.Enums.Warehouse;

namespace MultiWarehouse.Shared.DTOs.WarehouseZoneDtos
{
    public class WarehouseZoneCreateDto
    {
        public string ZoneName { get; set; } = string.Empty;
        public ZoneType ZoneType { get; set; }
        public Guid WarehouseId { get; set; }
    }
}