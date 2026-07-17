using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Shared.DTOs.StockDtos
{
    public class StockCreateDto
    {
        public Guid ProductId { get; set; }
        public Guid WarehouseId { get; set; }
        public Guid ShelfId { get; set; }
        public int Quantity { get; set; }
        // Yeni girişte rezervasyon genellikle 0'dır, ancak dışarıdan alınmak istenirse eklenebilir.
        public int ReservedQuantity { get; set; }
    }
}