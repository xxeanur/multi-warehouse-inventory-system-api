namespace MultiWarehouse.Shared.DTOs.DocumentDtos.TransferDtos
{
    public class TransferOrderCreateDto
    {
        public Guid SourceWarehouseId { get; set; }
        public Guid TargetWarehouseId { get; set; }
        public string Description { get; set; } = string.Empty;

        public List<TransferOrderLineCreateDto> Lines { get; set; } = new();
    }

    public class TransferOrderLineCreateDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}