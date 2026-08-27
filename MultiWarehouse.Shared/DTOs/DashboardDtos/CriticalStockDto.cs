namespace MultiWarehouse.Shared.DTOs.DashboardDtos
{
    public class CriticalStockDto
    {
        public Guid ProductId { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public int CriticalLevel { get; set; }
    }
}