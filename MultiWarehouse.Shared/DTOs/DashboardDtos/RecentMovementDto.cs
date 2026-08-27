namespace MultiWarehouse.Shared.DTOs.DashboardDtos
{
    public class RecentMovementDto
    {
        public Guid Id { get; set; }
        public string MovementType { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime MovementDate { get; set; }
        public string ReferenceNo { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string LocationInfo { get; set; } = string.Empty;
    }
}