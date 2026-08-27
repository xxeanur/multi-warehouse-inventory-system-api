using MultiWarehouse.Entity.Entities.Identity;
using MultiWarehouse.Entity.Enums.Common;

namespace MultiWarehouse.Entity.Entities.Common
{
    /// <summary>
    /// Sistemdeki tüm veri değişikliklerinin (Ekleme, Güncelleme, Silme) izini tutar. 
    /// Kurumsal projelerde "Kim, ne zaman, hangi tabloda, neyi değiştirdi?" sorusunun (Audit Log) cevabıdır.
    /// </summary>
    public class AuditLog : BaseEntity
    {
        public Guid UserId { get; set; } // İşlemi yapan kullanıcının kimliği
        public User User { get; set; } = null!;

        public AuditActionType ActionType { get; set; } // İşlem türü (Örn: Create, Update, Delete)
        public string TableName { get; set; } = string.Empty; // Hangi tabloda işlem yapıldığı (Örn: "Products")
        public string OldValues { get; set; } = string.Empty; // Güncellemeden önceki veri durumu (JSON formatında tutulabilir)
        public string NewValues { get; set; } = string.Empty; // Güncellemeden sonraki yeni veri durumu (JSON formatında tutulabilir)


        // İşlemi yapan kullanıcının o anki ağ adresi. Hesap çalınmalarını ve yetkisiz erişimleri tespit etmek için kritiktir.
        public string IpAddress { get; set; } = string.Empty;

    }
}