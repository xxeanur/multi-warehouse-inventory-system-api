using MultiWarehouse.Entity.Enums.Common;

namespace MultiWarehouse.Shared.DTOs.AuditLogDtos
{
    public class AuditLogDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public AuditActionType ActionType { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string OldValues { get; set; } = string.Empty;
        public string NewValues { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }

    }
}