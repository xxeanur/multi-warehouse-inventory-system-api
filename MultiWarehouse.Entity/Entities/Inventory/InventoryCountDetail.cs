using MultiWarehouse.Entity.Entities.Common;
using MultiWarehouse.Entity.Entities.Definitions;

namespace MultiWarehouse.Entity.Entities.Inventory
{

    public class InventoryCountDetail : BaseEntity
    {
        // Hangi ana sayım oturumuna ait?
        public Guid InventoryCountId { get; set; }
        public InventoryCount InventoryCount { get; set; } = null!;

        // Sayım yapılan fiziksel raf
        public Guid ShelfId { get; set; }
        public Shelf Shelf { get; set; } = null!;

        // Sayılan ürün
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;

        // Sistemdeki beklenen (teorik) miktar
        public int ExpectedQty { get; set; }

        // Personelin fiziksel olarak rafta bulup saydığı (gerçek) miktar
        public int CountedQty { get; set; }

        // Sayımda fark çıkarsa veya ürün hasarlıysa personelin girebileceği not
        public string Notes { get; set; } = string.Empty;
    }
}
