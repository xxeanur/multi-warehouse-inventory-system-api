namespace MultiWarehouse.Shared.DTOs.DocumentDtos.TransferDtos
{
    public class TransferOrderDetailDto : TransferOrderListDto
    {
        public string Description { get; set; } = string.Empty;
        public List<TransferOrderLineDto> Lines { get; set; } = new();
    }

    public class TransferOrderLineDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;

        public int ExpectedQuantity { get; set; }
        public int DispatchedQuantity { get; set; }
        public int ReceivedQuantity { get; set; }
        public List<TransferAllocationDto> Allocations { get; set; } = new();
    }

    public class TransferAllocationDto
    {
        public Guid SourceShelfId { get; set; }
        public string SourceShelfName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}