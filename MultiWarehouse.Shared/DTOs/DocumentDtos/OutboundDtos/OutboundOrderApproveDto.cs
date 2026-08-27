namespace MultiWarehouse.Shared.DTOs.DocumentDtos.OutboundDtos
{
    public class OutboundOrderApproveDto
    {
        public Guid OutboundOrderId { get; set; }

        public List<OutboundApproveLineDto> PickedLines { get; set; } = new();
    }

    public class OutboundApproveLineDto
    {
        public Guid OutboundOrderLineId { get; set; }
        public Guid ShelfId { get; set; }
        public int Quantity { get; set; }
    }
}