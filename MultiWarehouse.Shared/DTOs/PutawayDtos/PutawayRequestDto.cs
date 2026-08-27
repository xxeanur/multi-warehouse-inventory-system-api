namespace MultiWarehouse.Shared.DTOs.PutawayDtos
{
    public class PutawayRequestDto
    {
        public Guid DocumentId { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public Guid WarehouseId { get; set; }
        public List<PutawayLineDto> PlacedLines { get; set; } = new();
    }
}
