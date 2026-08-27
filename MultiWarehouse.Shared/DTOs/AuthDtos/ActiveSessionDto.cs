namespace MultiWarehouse.Shared.DTOs.AuthDtos
{
    public class ActiveSessionDto
    {
        public Guid Id { get; set; }
        public string DeviceName { get; set; } = string.Empty;
        public string Browser { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime LastAccessed { get; set; }
        public bool IsCurrentSession { get; set; }
    }
}