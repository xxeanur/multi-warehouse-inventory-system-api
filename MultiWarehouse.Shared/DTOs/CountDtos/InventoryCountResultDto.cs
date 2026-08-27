using MultiWarehouse.Entity.Enums.Inventory;

namespace MultiWarehouse.Shared.DTOs.CountDtos
{
    public class InventoryCountResultDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;

        public string ShelfCode { get; set; } = string.Empty;

        public int SystemQuantity { get; set; }
        public int CountedQuantity { get; set; }
        public int Variance { get; set; }

        public CountStatus Status { get; set; }        // 1: Matched, 2: Shortage, 3: Overage
        public string StatusName => Status switch
        {
            CountStatus.Matched => "Eşleşti",
            CountStatus.Shortage => "Eksik",
            CountStatus.Overage => "Fazla",
            _ => "Bilinmiyor"
        };
    }


}
