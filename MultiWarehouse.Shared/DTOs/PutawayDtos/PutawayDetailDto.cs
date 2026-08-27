namespace MultiWarehouse.Shared.DTOs.PutawayDtos
{
    public class PutawayDetailDto
    {
        public Guid DocumentId { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public List<PutawayDetailLineDto> Lines { get; set; } = new();
    }
}
