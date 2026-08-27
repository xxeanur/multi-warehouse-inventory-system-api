namespace MultiWarehouse.Shared.DTOs.StockDtos
{
    public class StockDto
    {
        public Guid Id { get; set; }

        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty; // EKLENDİ
        public string ProductCode { get; set; } = string.Empty; // EKLENDİ

        public Guid WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty; // EKLENDİ

        public Guid ShelfId { get; set; }
        public string ShelfCode { get; set; } = string.Empty; // EKLENDİ

        public int Quantity { get; set; }
        public int ReservedQuantity { get; set; }
        public DateTime? LastMovementDate { get; set; } // Anlık hareket olmayabilir diye Nullable yapıldı
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
    }
}