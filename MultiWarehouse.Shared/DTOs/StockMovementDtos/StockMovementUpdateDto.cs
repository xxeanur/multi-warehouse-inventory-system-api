using MultiWarehouse.Entity.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Shared.DTOs.StockMovementDtos
{
    public class StockMovementUpdateDto
    {
        public Guid Id { get; set; }
        public string ReferenceNo { get; set; } = string.Empty;
        public MovementStatus Status { get; set; }
        public string Description { get; set; } = string.Empty;
        // Not: Gerçek bir WMS'de hareket miktarı veya ürün değiştirilemez. 
        // Sadece durum (Status) veya açıklama (Description) güncellenmelidir.
    }
}