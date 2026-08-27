namespace MultiWarehouse.Shared.DTOs.DocumentDtos.OutboundDtos
{
    public class OutboundOrderDetailDto : OutboundOrderListDto
    {
        public string Description { get; set; } = string.Empty;
        public List<OutboundOrderLineDto> Lines { get; set; } = new();
    }

    public class OutboundOrderLineDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;

        public int RequestedQuantity { get; set; }
        public int PickedQuantity { get; set; }
        public string PickedShelf { get; set; } = string.Empty;

        public List<OutboundAllocationDto> Allocations { get; set; } = new();
    }

    public class OutboundAllocationDto
    {
        public Guid ShelfId { get; set; }
        public string ShelfName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}