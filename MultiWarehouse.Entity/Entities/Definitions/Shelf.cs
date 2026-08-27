using MultiWarehouse.Entity.Entities.Common;
using MultiWarehouse.Entity.Enums.Warehouse;
using System.ComponentModel.DataAnnotations;

namespace MultiWarehouse.Entity.Entities.Definitions
{
    /// <summary>
    /// Deponun içindeki fiziksel rafları temsil eder. 
    /// Kapasite (Hacim/Ağırlık) yönetimi burada yapılır, limitsiz ürün konulması engellenir.
    /// </summary>
    public class Shelf : BaseEntity
    {
        public string ShelfNumber { get; set; } = string.Empty;

        // Fiziksel Boyutlar (Senin müdahalenle eklenen kısım)
        public double Width { get; set; }  // Genişlik
        public double Height { get; set; } // Yükseklik
        public double Depth { get; set; }  // Derinlik

        public double MaxVolume { get; set; }
        public double MaxWeight { get; set; }

        // Rafın anlık olarak ne kadarının dolu olduğu 
        public double CurrentVolume { get; set; } = 0;
        public double CurrentWeight { get; set; } = 0;

        public ShelfStatus Status { get; set; } = ShelfStatus.Available;

        // İlişki
        public Guid WarehouseZoneId { get; set; }
        public WarehouseZone WarehouseZone { get; set; } = null!;

        //eşzamanlılık
        [ConcurrencyCheck]
        public Guid Version { get; set; } = Guid.NewGuid();
    }
}
