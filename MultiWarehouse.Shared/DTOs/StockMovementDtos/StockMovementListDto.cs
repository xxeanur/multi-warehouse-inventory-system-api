using MultiWarehouse.Entity.Enums.Inventory;

namespace MultiWarehouse.Shared.DTOs.StockMovementDtos
{
    public class StockMovementListDto
    {
        public Guid Id { get; set; }

        public Guid WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;

        public Guid ShelfId { get; set; }
        public string ShelfCode { get; set; } = string.Empty;

        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;

        public MovementType MovementType { get; set; }

        public string MovementDirection { get; set; } = string.Empty;
        public string MovementTypeName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public Guid? DocumentId { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public Guid UserId { get; set; }

        public string OperatorName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}