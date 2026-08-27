namespace MultiWarehouse.Shared.DTOs.DocumentDtos.TransferDtos
{
    public class TransferOrderDispatchDto
    {
        public Guid TransferOrderId { get; set; }

        public List<TransferDispatchLineDto> DispatchedLines { get; set; } = new();
    }

    public class TransferDispatchLineDto
    {
        public Guid TransferOrderLineId { get; set; }
        public Guid SourceShelfId { get; set; }
        public int Quantity { get; set; }
    }
}