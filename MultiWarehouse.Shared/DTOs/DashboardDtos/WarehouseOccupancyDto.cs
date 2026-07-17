using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Shared.DTOs.DashboardDtos
{
    public class WarehouseOccupancyDto
    {
        public string WarehouseName { get; set; } = string.Empty;
        public double UsedCapacity { get; set; }
        public double MaxCapacity { get; set; }
        public double OccupancyRate { get; set; } // Yüzdelik doluluk oranı (Örn: %75.5)
    }
}