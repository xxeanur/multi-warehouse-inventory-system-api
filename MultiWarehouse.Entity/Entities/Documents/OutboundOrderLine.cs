using MultiWarehouse.Entity.Entities.Common;
using MultiWarehouse.Entity.Entities.Definitions;

namespace MultiWarehouse.Entity.Entities.Documents
{
    public class OutboundOrderLine : BaseEntity
    {
        public Guid OutboundOrderId { get; set; }
        public Guid ProductId { get; set; }

        public int RequestedQuantity { get; set; } // İstenen / Sipariş Edilen Miktar
        public int PickedQuantity { get; set; } = 0; // Gerçekte Raflardan Toplanan Miktar

        // Navigation Properties
        public OutboundOrder? OutboundOrder { get; set; }
        public Product? Product { get; set; }
    }
}
