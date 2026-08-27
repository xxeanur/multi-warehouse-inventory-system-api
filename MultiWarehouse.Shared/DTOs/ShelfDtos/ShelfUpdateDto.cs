using MultiWarehouse.Entity.Enums.Warehouse;

namespace MultiWarehouse.Shared.DTOs.ShelfDtos
{
    public class ShelfUpdateDto
    {
        public Guid Id { get; set; }
        public string ShelfNumber { get; set; } = string.Empty;
        public double Width { get; set; }
        public double Height { get; set; }
        public double Depth { get; set; }
        public double MaxWeight { get; set; }
        public ShelfStatus Status { get; set; }
        public Guid WarehouseZoneId { get; set; }
    }
}