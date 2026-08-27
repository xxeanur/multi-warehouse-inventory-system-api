using MultiWarehouse.Entity.Entities.Common;
using MultiWarehouse.Entity.Entities.Definitions;

namespace MultiWarehouse.Entity.Entities.Documents
{
    public class OutboundOrderReservation : BaseEntity
    {
        public Guid OutboundOrderId { get; set; }
        public Guid ProductId { get; set; }
        public Guid ShelfId { get; set; }

        public int ReservedQuantity { get; set; }

        // Navigation Properties
        public OutboundOrder? OutboundOrder { get; set; }
        public Product? Product { get; set; }
        public Shelf? Shelf { get; set; }
    }
}