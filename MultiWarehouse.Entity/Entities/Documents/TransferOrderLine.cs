using MultiWarehouse.Entity.Entities.Common;
using MultiWarehouse.Entity.Entities.Definitions;

namespace MultiWarehouse.Entity.Entities.Documents
{
    public class TransferOrderLine : BaseEntity
    {
        public Guid TransferOrderId { get; set; }
        public Guid ProductId { get; set; }

        public int ExpectedQuantity { get; set; } // Başlangıçta istenen/beklenen miktar
        public int DispatchedQuantity { get; set; } = 0; // Kaynak depodan yola çıkan miktar 
        public int ReceivedQuantity { get; set; } = 0; // Hedef depoya sağ salim varan miktar

        // Navigation Properties
        public TransferOrder? TransferOrder { get; set; }
        public Product? Product { get; set; }
    }
}