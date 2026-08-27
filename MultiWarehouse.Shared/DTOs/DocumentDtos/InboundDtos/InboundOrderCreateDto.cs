using MultiWarehouse.Entity.Enums.Inventory;

namespace MultiWarehouse.Shared.DTOs.DocumentDtos.InboundDtos
{
    public class InboundOrderCreateDto
    {

        public Guid? SupplierId { get; set; }
        public Guid WarehouseId { get; set; }
        public MovementType MovementType { get; set; }
        public string Description { get; set; } = string.Empty;

        public List<InboundOrderLineCreateDto> Lines { get; set; } = new();
    }

    public class InboundOrderLineCreateDto
    {
        public Guid ProductId { get; set; }
        public int ExpectedQuantity { get; set; }
    }
}