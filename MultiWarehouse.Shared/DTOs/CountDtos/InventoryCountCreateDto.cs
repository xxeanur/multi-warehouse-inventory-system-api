namespace MultiWarehouse.Shared.DTOs.CountDtos
{
    public class InventoryCountCreateDto
    {
        public Guid WarehouseId { get; set; }
        public Guid ShelfId { get; set; }
        public Guid ProductId { get; set; }
        public int CountedQuantity { get; set; }
    }
}