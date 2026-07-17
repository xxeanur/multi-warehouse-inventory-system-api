using MultiWarehouse.Entity.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Shared.DTOs.ProductDtos
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public double Width { get; set; }
        public double Height { get; set; }
        public double Depth { get; set; }
        public double Weight { get; set; }
        public string Barcode { get; set; } = string.Empty;

        // GÜNCELLENDİ
        public UnitType Unit { get; set; }

        public decimal UnitPrice { get; set; }
        public decimal CostPrice { get; set; }
        public int CriticalLevel { get; set; }
        public Guid CategoryId { get; set; }
        public Guid SupplierId { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
    }
}