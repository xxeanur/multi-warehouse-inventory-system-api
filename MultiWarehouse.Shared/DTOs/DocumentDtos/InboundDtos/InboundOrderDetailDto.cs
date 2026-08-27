namespace MultiWarehouse.Shared.DTOs.DocumentDtos.InboundDtos
{
    public class InboundOrderDetailDto : InboundOrderListDto
    {
        public string Description { get; set; } = string.Empty;
        public List<InboundOrderLineDto> Lines { get; set; } = new();
    }

    public class InboundOrderLineDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;

        public int ExpectedQuantity { get; set; }
        public int ReceivedQuantity { get; set; }
    }
}
