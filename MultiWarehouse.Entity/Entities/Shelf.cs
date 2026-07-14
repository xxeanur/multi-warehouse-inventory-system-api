using MultiWarehouse.Entity.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Entity.Entities
{
    /// <summary>
    /// Deponun içindeki fiziksel rafları temsil eder. 
    /// Kapasite (Hacim/Ağırlık) yönetimi burada yapılır, limitsiz ürün konulması engellenir.
    /// </summary>
    public class Shelf : BaseEntity
    {
        public string ShelfNumber { get; set; } = string.Empty; // Raf kodu (Örn: A-Blok-01)

        // Fiziksel Boyutlar (Senin müdahalenle eklenen kısım)
        public double Width { get; set; }  // Genişlik
        public double Height { get; set; } // Yükseklik
        public double Depth { get; set; }  // Derinlik

        // Rafın maksimum fiziksel taşıma kapasiteleri
        public double MaxVolume { get; set; }
        public double MaxWeight { get; set; }

        // Rafın anlık olarak ne kadarının dolu olduğu (Ürün eklendikçe artar, çıktıkça azalır)
        public double CurrentVolume { get; set; } = 0;
        public double CurrentWeight { get; set; } = 0;

        // Rafın anlık durumu. Örnek kullanımlar:
        // "Available"   : Kullanıma hazır, boş yer varsa ürün konulabilir.
        // "Maintenance" : Raf hasarlı veya bakımda, ürün konulamaz/alınamaz.
        // "Reserved"    : Gelecek bir mal kabul (Inbound) işlemi için önceden ayrılmış.
        // GÜNCELLENEN KISIM: Raf durumu artık tip güvenli.
        public ShelfStatus Status { get; set; } = ShelfStatus.Available;

        // İlişki: Bu rafın hangi depo bloğunda (Zone) yer aldığı bilgisi
        public Guid WarehouseZoneId { get; set; }
        public WarehouseZone WarehouseZone { get; set; } = null!;
    }
}
