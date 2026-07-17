using MultiWarehouse.Entity.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Shared.DTOs.StockMovementDtos
{
    public class StockMovementCreateDto
    {
        public Guid ProductId { get; set; }
        public MovementType MovementType { get; set; }
        public int Quantity { get; set; }
        public string ReferenceNo { get; set; } = string.Empty;
        public DateTime MovementDate { get; set; } = DateTime.UtcNow;
        public MovementStatus Status { get; set; } = MovementStatus.Completed;
        public Guid? SourceWarehouseId { get; set; }
        public Guid? DestinationWarehouseId { get; set; }
        public Guid? SourceShelfId { get; set; }
        public Guid? DestinationShelfId { get; set; }
        public Guid UserId { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}