using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Shared.DTOs.DashboardDtos
{
    public class RecentMovementDto
    {
        public string MovementType { get; set; } = string.Empty; // Inbound, Outbound vs.
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime MovementDate { get; set; }
        public string ReferenceNo { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
    }
}