using MultiWarehouse.Entity.Enums.Warehouse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Shared.DTOs.ShelfDtos
{
    public class ShelfDto
    {
        public Guid Id { get; set; }
        public string ShelfNumber { get; set; } = string.Empty;
        public double Width { get; set; }
        public double Height { get; set; }
        public double Depth { get; set; }
        public double MaxVolume { get; set; }
        public double MaxWeight { get; set; }
        public double CurrentVolume { get; set; }
        public double CurrentWeight { get; set; }
        public ShelfStatus Status { get; set; }
        public Guid WarehouseZoneId { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
    }
}