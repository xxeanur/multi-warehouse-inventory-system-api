using MultiWarehouse.Entity.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Entity.Entities
{
    /// <summary>
    /// Depoda yapılan periyodik veya anlık sayım oturumlarının başlık bilgisini tutar.
    /// </summary>
    public class InventoryCount : BaseEntity
    {
        // Sayımın yapıldığı depo
        public Guid WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; } = null!;

        // Sayımı başlatan veya sorumlu olan personel/yönetici
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime StartedAt { get; set; } // Sayımın başladığı an
        public DateTime? CompletedAt { get; set; } // Sayımın bittiği an (Henüz bitmediyse null olabilir)

        // Sayımın durumu (Type-Safe Enum)
        public CountStatus Status { get; set; } = CountStatus.Planned;

        // Bu sayım oturumuna ait detay satırları (Hangi rafta hangi ürün sayıldı?)
        public List<InventoryCountDetail> CountDetails { get; set; } = new List<InventoryCountDetail>();
    }
}
