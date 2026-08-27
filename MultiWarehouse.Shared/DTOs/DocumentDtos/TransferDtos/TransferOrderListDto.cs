using MultiWarehouse.Entity.Enums.Document;

namespace MultiWarehouse.Shared.DTOs.DocumentDtos.TransferDtos
{
    public class TransferOrderListDto
    {
        public Guid Id { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;

        public Guid SourceWarehouseId { get; set; }
        public Guid TargetWarehouseId { get; set; }

        public string SourceWarehouseName { get; set; } = string.Empty;
        public string TargetWarehouseName { get; set; } = string.Empty;

        public DocumentStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public DateTime CreatedDate { get; set; }

        public string? CreatedByName { get; set; }
        public string? DispatchedByName { get; set; }
        public string? ReceivedByName { get; set; }
        public string? CancelledByName { get; set; }
    }
}