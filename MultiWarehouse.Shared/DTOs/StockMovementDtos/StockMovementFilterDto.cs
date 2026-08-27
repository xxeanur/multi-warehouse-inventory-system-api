using MultiWarehouse.Entity.Enums.Inventory;

namespace MultiWarehouse.Shared.DTOs.StockMovementDtos
{
    public class StockMovementFilterDto
    {
        public Guid? WarehouseId { get; set; }
        public Guid? ShelfId { get; set; }
        public Guid? ProductId { get; set; }

        public string? Direction { get; set; }

        public MovementType? MovementType { get; set; }

        public Guid? DocumentId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string? SearchTerm { get; set; }
    }
}