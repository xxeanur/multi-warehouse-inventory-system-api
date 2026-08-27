using MultiWarehouse.Entity.Enums.Common;

namespace MultiWarehouse.Shared.DTOs.SearchDtos
{
    public class SearchResultItemDto
    {
        public string Category { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;

        public SearchTargetType TargetType { get; set; }
        public Guid TargetId { get; set; }
    }
}