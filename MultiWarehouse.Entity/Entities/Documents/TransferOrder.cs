using MultiWarehouse.Entity.Entities.Common;
using MultiWarehouse.Entity.Entities.Definitions;
using MultiWarehouse.Entity.Entities.Identity;
using MultiWarehouse.Entity.Enums.Document;

namespace MultiWarehouse.Entity.Entities.Documents
{
    public class TransferOrder : BaseEntity
    {
        public string DocumentNumber { get; set; } = string.Empty;
        public Guid SourceWarehouseId { get; set; }
        public Guid TargetWarehouseId { get; set; }
        public string Description { get; set; } = string.Empty;
        public DocumentStatus Status { get; set; } = DocumentStatus.Pending;


        public Guid? CreatedById { get; set; }
        public User? CreatedBy { get; set; }

        public Guid? DispatchedById { get; set; }
        public User? DispatchedBy { get; set; } // Yola Çıkaran

        public Guid? ReceivedById { get; set; }
        public User? ReceivedBy { get; set; } // Teslim Alan

        public Guid? CancelledById { get; set; }
        public User? CancelledBy { get; set; }


        public Warehouse? SourceWarehouse { get; set; }
        public Warehouse? TargetWarehouse { get; set; }
        public ICollection<TransferOrderLine> Lines { get; set; } = new List<TransferOrderLine>();
        public ICollection<TransferOrderReservation> Reservations { get; set; } = new List<TransferOrderReservation>();
    }
}