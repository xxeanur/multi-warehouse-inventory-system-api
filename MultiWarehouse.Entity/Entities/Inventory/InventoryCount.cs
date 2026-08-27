using MultiWarehouse.Entity.Entities.Common;
using MultiWarehouse.Entity.Entities.Definitions;
using MultiWarehouse.Entity.Enums.Inventory;

namespace MultiWarehouse.Entity.Entities.Inventory
{

    public class InventoryCount : BaseEntity
    {
        public Guid WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }

        public Guid ShelfId { get; set; }
        public Shelf? Shelf { get; set; }

        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        public int SystemQuantity { get; set; }  // Sayım anında sistemdeki miktar
        public int CountedQuantity { get; set; } // Operatörün fiziksel saydığı miktar
        public int Variance { get; set; }        // Fark (CountedQuantity - SystemQuantity)

        public CountStatus Status { get; set; }  // Matched, Shortage, Overage

        public string Description { get; set; } = string.Empty;
        public Guid UserId { get; set; }         // Sayımı yapan operatör
    }
}