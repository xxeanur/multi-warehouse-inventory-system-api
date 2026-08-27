using MultiWarehouse.Entity.Enums.Inventory;

namespace MultiWarehouse.Shared.DTOs.DocumentDtos.OutboundDtos
{
    public class OutboundOrderCreateDto
    {
        public Guid WarehouseId { get; set; }
        public string Destination { get; set; } = string.Empty;
        public MovementType MovementType { get; set; }
        public string Description { get; set; } = string.Empty;

        public List<OutboundOrderLineCreateDto> Lines { get; set; } = new();
    }

    public class OutboundOrderLineCreateDto
    {
        public Guid ProductId { get; set; }
        public int RequestedQuantity { get; set; }
    }
}