using MultiWarehouse.Entity.Enums.Document;
using MultiWarehouse.Entity.Enums.Inventory;

namespace MultiWarehouse.Shared.DTOs.DocumentDtos.InboundDtos
{
    public class InboundOrderListDto
    {
        public Guid Id { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public string? SupplierName { get; set; }

        public Guid WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public MovementType MovementType { get; set; }
        public string MovementTypeName => MovementType.ToString();
        public DocumentStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public DateTime CreatedDate { get; set; }

        public Guid? SourceTransferOrderId { get; set; }

        public string? CreatedByName { get; set; }
        public string? ApprovedByName { get; set; }
        public string? CancelledByName { get; set; }
    }
}