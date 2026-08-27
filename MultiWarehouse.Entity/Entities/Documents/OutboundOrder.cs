using MultiWarehouse.Entity.Entities.Common;
using MultiWarehouse.Entity.Entities.Definitions;
using MultiWarehouse.Entity.Entities.Identity; // YENİ: User bağlantısı
using MultiWarehouse.Entity.Enums.Document;
using MultiWarehouse.Entity.Enums.Inventory;

namespace MultiWarehouse.Entity.Entities.Documents
{
    public class OutboundOrder : BaseEntity
    {
        public string DocumentNumber { get; set; } = string.Empty;

        public MovementType MovementType { get; set; } = MovementType.Outbound;
        public Guid WarehouseId { get; set; }
        public string Destination { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public DocumentStatus Status { get; set; } = DocumentStatus.Pending;

        public Guid? CreatedById { get; set; }
        public User? CreatedBy { get; set; }

        public Guid? ApprovedById { get; set; }
        public User? ApprovedBy { get; set; }

        public Guid? CancelledById { get; set; }
        public User? CancelledBy { get; set; }

        // Navigation Properties
        public Warehouse? Warehouse { get; set; }
        public ICollection<OutboundOrderLine> Lines { get; set; } = new List<OutboundOrderLine>();
        public ICollection<OutboundOrderReservation> Reservations { get; set; } = new List<OutboundOrderReservation>();
    }
}