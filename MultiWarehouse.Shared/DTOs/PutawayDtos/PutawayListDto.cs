namespace MultiWarehouse.Shared.DTOs.PutawayDtos
{
    public class PutawayListDto
    {
        public Guid DocumentId { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string MovementTypeName { get; set; } = string.Empty;
        public string SourceName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}
