using MultiWarehouse.Entity.Entities.Common;
using MultiWarehouse.Entity.Entities.Definitions;

namespace MultiWarehouse.Entity.Entities.Documents
{

    public class TransferOrderReservation : BaseEntity
    {
        public Guid TransferOrderId { get; set; }
        public Guid ProductId { get; set; }
        public Guid SourceShelfId { get; set; } // Kaynak depodaki raf

        public int ReservedQuantity { get; set; }

        // Navigation Properties
        public TransferOrder? TransferOrder { get; set; }
        public Product? Product { get; set; }
        public Shelf? SourceShelf { get; set; }
    }
}