using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Shared.DTOs.StockDtos
{
    public class StockUpdateDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid WarehouseId { get; set; }
        public Guid ShelfId { get; set; }
        public int Quantity { get; set; }
        public int ReservedQuantity { get; set; }
    }
}