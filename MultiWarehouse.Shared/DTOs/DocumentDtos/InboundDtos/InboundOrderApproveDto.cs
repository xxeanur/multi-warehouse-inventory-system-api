namespace MultiWarehouse.Shared.DTOs.DocumentDtos.InboundDtos
{
    public class InboundOrderApproveDto
    {
        public Guid InboundOrderId { get; set; }
        public List<InboundApproveLineDto> ApprovedLines { get; set; } = new();
    }

    public class InboundApproveLineDto
    {
        public Guid InboundOrderLineId { get; set; }
        public int ReceivedQuantity { get; set; }
    }
}