using MultiWarehouse.Entity.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Shared.DTOs.WarehouseZoneDtos
{
    public class WarehouseZoneUpdateDto
    {
        public Guid Id { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        public ZoneType ZoneType { get; set; }
        public Guid WarehouseId { get; set; }
    }
}
