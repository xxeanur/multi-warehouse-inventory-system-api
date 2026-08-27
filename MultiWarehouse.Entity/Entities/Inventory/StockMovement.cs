using MultiWarehouse.Entity.Entities.Common;
using MultiWarehouse.Entity.Entities.Definitions;
using MultiWarehouse.Entity.Entities.Identity;
using MultiWarehouse.Entity.Enums.Inventory; // MovementType burada

namespace MultiWarehouse.Entity.Entities.Inventory
{

    public class StockMovement : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;

        // HAREKETİN GERÇEKLEŞTİĞİ TEK BİR DEPO VE RAF VARDIR
        public Guid WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; } = null!;

        public Guid ShelfId { get; set; }
        public Shelf Shelf { get; set; } = null!;

        // HAREKETİN NEDENİ (Inbound, Outbound, Scrap, SupplierReturn vs.)
        public MovementType MovementType { get; set; }

        // Miktar 
        public int Quantity { get; set; }

        // --- İZLENEBİLİRLİK (AUDIT TRAIL) - FİŞ BAĞLANTISI ---

        public Guid DocumentId { get; set; }

        // "InboundOrder", "TransferOrder", "OutboundOrder" gibi belgenin tablosunu belirtir
        public string DocumentType { get; set; } = string.Empty;

        public bool IsCancelled { get; set; } = false;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public string Description { get; set; } = string.Empty; // Opsiyonel açıklama
    }
}