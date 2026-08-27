using MultiWarehouse.Shared.DTOs.StockMovementDtos;

namespace MultiWarehouse.Shared.DTOs.InventoryDtos
{
    public class StockMovementDetailDto : StockMovementListDto
    {
        // Çekmece (Drawer) açıldığında fazladan gösterilecek veriler
        public string DocumentReference { get; set; } = string.Empty;
        public string OperatorEmail { get; set; } = string.Empty;
        public string OperatorRole { get; set; } = string.Empty;
        public bool IsCancelled { get; set; }
    }
}