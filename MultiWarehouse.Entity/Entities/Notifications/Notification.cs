using MultiWarehouse.Entity.Entities.Common;
using MultiWarehouse.Entity.Entities.Identity;
using MultiWarehouse.Entity.Enums.Common;

namespace MultiWarehouse.Entity.Entities.Notification
{

    public class Notification : BaseEntity
    {
        public Guid UserId { get; set; } // Bildirimin gideceği kullanıcı
        public User User { get; set; } = null!;
        public string Title { get; set; } = string.Empty; // Bildirim başlığı
        public string Message { get; set; } = string.Empty; // Bildirim içeriği
        public bool IsRead { get; set; } = false; // Bildirimin okunup okunmadığı durumu

        // Bildirimin türü 
        public NotificationType Type { get; set; }

        public NotificationTargetType TargetType { get; set; } = NotificationTargetType.None;

        // O modüldeki hangi kaydın detayına gidilecek? 
        public Guid? TargetId { get; set; }
    }
}
