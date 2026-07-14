using MultiWarehouse.Entity.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Entity.Entities
{
    /// <summary>
    /// Sistem içindeki kullanıcılara gönderilen duyuru, uyarı ve bilgilendirmeleri tutar.
    /// Örneğin: "Stok kritik seviyenin altında!", "Yeni ürün girişi yapıldı" gibi alarmlar.
    /// </summary>
    public class Notification : BaseEntity
    {
        public Guid UserId { get; set; } // Bildirimin gideceği kullanıcı
        public User User { get; set; } = null!;
        public string Title { get; set; } = string.Empty; // Bildirim başlığı
        public string Message { get; set; } = string.Empty; // Bildirim içeriği
        public bool IsRead { get; set; } = false; // Bildirimin okunup okunmadığı durumu
        // --- YENİ EKLENEN AKILLI ALANLAR ---

        // Bildirimin türü (İkon ve renk belirlemek için kullanılır)
        public NotificationType Type { get; set; }

        // Kullanıcı bildirime tıkladığında yönlendirileceği sayfanın adresi (Örn: "/products/5")
        public string Url { get; set; } = string.Empty;
    }
}
