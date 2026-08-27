using MultiWarehouse.Entity.Entities.Common;
using MultiWarehouse.Entity.Entities.Definitions;

namespace MultiWarehouse.Entity.Entities.Documents
{
    public class InboundOrderLine : BaseEntity
    {
        public Guid InboundOrderId { get; set; }
        public Guid ProductId { get; set; }
        public int ExpectedQuantity { get; set; }
        public int ReceivedQuantity { get; set; } = 0;

        public InboundOrder? InboundOrder { get; set; }
        public Product? Product { get; set; }
    }
}