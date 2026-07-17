using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Shared.DTOs.DashboardDtos
{
    public class CriticalStockDto
    {
        public Guid ProductId { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int TotalQuantity { get; set; } // Tüm depolardaki toplam adet
        public int CriticalLevel { get; set; } // Alarm seviyesi
    }
}