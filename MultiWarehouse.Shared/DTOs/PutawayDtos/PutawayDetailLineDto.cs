namespace MultiWarehouse.Shared.DTOs.PutawayDtos
{
    public class PutawayDetailLineDto
    {
        public Guid DocumentLineId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public int QuantityToPlace { get; set; }
    }
}
