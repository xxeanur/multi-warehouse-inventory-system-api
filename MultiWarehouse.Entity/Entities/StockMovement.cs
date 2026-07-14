using MultiWarehouse.Entity.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Entity.Entities
{

    /// <summary>
    /// Depoya giren, çıkan veya transfer edilen tüm stok hareketlerinin mali defteridir.
    /// </summary>
    public class StockMovement : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public MovementType MovementType { get; set; } // "Inbound", "Outbound", "WarehouseTransfer", "ShelfTransfer"

        public int Quantity { get; set; }

        // --- YENİ EKLENEN LOJİSTİK VE KONTROL ALANLARI ---

        // İşlemin dayandığı resmi evrak veya sipariş numarası (İrsaliye, Fatura, Sipariş No)
        public string ReferenceNo { get; set; } = string.Empty;

        // Malın fiziksel olarak hareket ettiği gerçek tarih/saat
        public DateTime MovementDate { get; set; }

        // Hareketin anlık durumu (Örn: "Pending", "InTransit", "Completed", "Cancelled")
        public MovementStatus Status { get; set; } = MovementStatus.Completed;


        // DEPOLAR ARASI TRANSFER İÇİN (Konya'dan çıktı, Ankara'ya girdi)
        public Guid? SourceWarehouseId { get; set; }
        public Guid? DestinationWarehouseId { get; set; }

        // RAFLAR ARASI TRANSFER İÇİN (Aynı depo içinde yer değiştirme)
        public Guid? SourceShelfId { get; set; }
        public Guid? DestinationShelfId { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public string Description { get; set; } = string.Empty;
    }
}

