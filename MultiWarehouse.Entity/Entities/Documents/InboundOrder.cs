using MultiWarehouse.Entity.Entities.Common;
using MultiWarehouse.Entity.Entities.Definitions;
using MultiWarehouse.Entity.Entities.Identity; // YENİ: User tablosunu tanıyabilmesi için eklendi
using MultiWarehouse.Entity.Enums.Document;
using MultiWarehouse.Entity.Enums.Inventory;

namespace MultiWarehouse.Entity.Entities.Documents
{
    public class InboundOrder : BaseEntity
    {
        public string DocumentNumber { get; set; } = string.Empty;
        public Guid? SupplierId { get; set; }
        public Guid WarehouseId { get; set; }
        public string Description { get; set; } = string.Empty;
        public MovementType MovementType { get; set; } = MovementType.Inbound;
        public DocumentStatus Status { get; set; } = DocumentStatus.Pending;
        public Guid? SourceTransferOrderId { get; set; }
        public TransferOrder? SourceTransferOrder { get; set; }

        public Guid? CreatedById { get; set; }
        public User? CreatedBy { get; set; }

        public Guid? ApprovedById { get; set; }
        public User? ApprovedBy { get; set; }

        public Guid? CancelledById { get; set; }
        public User? CancelledBy { get; set; }

        // Navigation Properties
        public Supplier? Supplier { get; set; }
        public Warehouse? Warehouse { get; set; }
        public ICollection<InboundOrderLine> Lines { get; set; } = new List<InboundOrderLine>();
    }
}