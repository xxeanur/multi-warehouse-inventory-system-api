using MultiWarehouse.Entity.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Shared.DTOs.ShelfDtos
{
    public class ShelfCreateDto
    {
        public string ShelfNumber { get; set; } = string.Empty;
        public double Width { get; set; }
        public double Height { get; set; }
        public double Depth { get; set; }
        public double MaxVolume { get; set; }
        public double MaxWeight { get; set; }
        public ShelfStatus Status { get; set; } = ShelfStatus.Available;
        public Guid WarehouseZoneId { get; set; }
    }
}