using MultiWarehouse.Entity.Entities.Common;
using MultiWarehouse.Entity.Enums.Warehouse;

namespace MultiWarehouse.Entity.Entities.Definitions
{/// <summary>
 /// Deponun içindeki ana blokları, koridorları veya soğuk hava odalarını temsil eder.
 /// </summary>
    public class WarehouseZone : BaseEntity
    {
        public string ZoneName { get; set; } = string.Empty;

        public ZoneType ZoneType { get; set; } = ZoneType.General;

        public Guid WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; } = null!;

        public List<Shelf> Shelves { get; set; } = new List<Shelf>();
    }
}

