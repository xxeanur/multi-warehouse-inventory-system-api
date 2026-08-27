using MultiWarehouse.Entity.Enums.Warehouse;

namespace MultiWarehouse.Shared.DTOs.WarehouseZoneDtos
{
    public class WarehouseZoneDto
    {
        public Guid Id { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        public ZoneType ZoneType { get; set; }
        public Guid WarehouseId { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
    }
}