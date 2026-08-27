using MultiWarehouse.Entity.Entities.Common;
using MultiWarehouse.Entity.Entities.Definitions;
using System.ComponentModel.DataAnnotations;

namespace MultiWarehouse.Entity.Entities.Inventory
{

    public class Stock : BaseEntity
    {
        // Hangi ürün?
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;

        // Hangi depoda?
        public Guid WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; } = null!;

        // O deponun hangi rafında?
        public Guid ShelfId { get; set; }
        public Shelf Shelf { get; set; } = null!;

        // Belirtilen raftaki miktar
        public int Quantity { get; set; }

        // EKLENEN KISIM
        // Siparişi alınmış ama henüz kargolanmamış, başkasına satılamayacak stok miktarı
        public int ReservedQuantity { get; set; }

        public DateTime LastMovementDate { get; set; }

        [ConcurrencyCheck]
        public Guid Version { get; set; } = Guid.NewGuid();
    }
}