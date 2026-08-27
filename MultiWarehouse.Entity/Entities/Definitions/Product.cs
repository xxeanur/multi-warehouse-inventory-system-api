using MultiWarehouse.Entity.Entities.Common;
using MultiWarehouse.Entity.Entities.Inventory;
using MultiWarehouse.Entity.Enums.Product;

namespace MultiWarehouse.Entity.Entities.Definitions
{
    /// <summary>
    /// Depodaki temel yapı taşımız olan ürünleri temsil eder. 
    /// WMS (Depo Yönetimi) sistemlerinde sadece isim/fiyat değil; ebat, ağırlık ve stok limitleri çok kritiktir.
    /// </summary>
    public class Product : BaseEntity
    {
        public string Sku { get; set; } = string.Empty; // Stok Tutma Birimi 
        public string Name { get; set; } = string.Empty; // Ürün Adı

        public string Brand { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;

        // Fiziksel Özellikler 
        public double Width { get; set; }  // Genişlik
        public double Height { get; set; } // Yükseklik
        public double Depth { get; set; }  // Derinlik
        public double Weight { get; set; } // Ağırlık

        // Ürünün Toplam Hacmi (Genişlik x Yükseklik x Derinlik) - Sadece okunabilir (Read-Only)
        public double Volume => Width * Height * Depth;

        // Ürünün fiziksel barkod numarası
        public string Barcode { get; set; } = string.Empty;

        // Ürünün sayım veya satış birimi (Örn: "Adet", "Kg", "Litre", "Koli")
        public UnitType Unit { get; set; } = UnitType.Piece;

        // Ürünün birim maliyeti/fiyatı 
        public decimal UnitPrice { get; set; }
        public decimal CostPrice { get; set; }//satış fiyatı

        public int CriticalLevel { get; set; } // Ürün bu sayının altına düştüğünde sistem alarm (Notification) verir

        // İlişkiler: Bu ürün hangi kategoriye ait ve hangi tedarikçiden geliyor?
        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public Guid SupplierId { get; set; }
        public Supplier Supplier { get; set; } = null!;

        // Bir ürünün farklı depolarda/raflarda birden fazla stok kaydı olabilir
        public List<Stock> Stocks { get; set; } = new List<Stock>();
    }
}
