using MultiWarehouse.Entity.Enums.Common;

namespace MultiWarehouse.Shared.DTOs.NotificationDtos
{
    public class NotificationCreateDto
    {
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public NotificationTargetType TargetType { get; set; } = NotificationTargetType.None;
        public Guid? TargetId { get; set; }
    }
}