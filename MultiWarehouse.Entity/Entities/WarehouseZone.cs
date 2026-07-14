
using MultiWarehouse.Entity.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Entity.Entities
{/// <summary>
 /// Deponun içindeki ana blokları, koridorları veya soğuk hava odalarını temsil eder.
 /// </summary>
    public class WarehouseZone : BaseEntity
    {
        public string ZoneName { get; set; } = string.Empty;

        // GÜNCELLENEN KISIM: Category kelimesi yerine ZoneType kullanıyoruz.
        // Örn: "Cold Storage", "Chemical", "General"
        public ZoneType ZoneType { get; set; } = ZoneType.General;

        // YENİ EKLENEN KISIM: Bu blok hangi Depoya (Warehouse) ait?
        public Guid WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; } = null!;

        public List<Shelf> Shelves { get; set; } = new List<Shelf>();
    }
}

