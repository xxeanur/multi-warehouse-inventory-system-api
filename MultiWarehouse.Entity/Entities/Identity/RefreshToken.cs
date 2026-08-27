using MultiWarehouse.Entity.Entities.Common;

namespace MultiWarehouse.Entity.Entities.Identity
{
    /// <summary>
    /// Kullanıcının sürekli login olmasını engellemek için arka planda JWT yenilemeye yarayan uzun ömürlü anahtardır.
    /// </summary>
    public class RefreshToken : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public string Token { get; set; } = string.Empty;
        public DateTime Expires { get; set; }
        public bool IsRevoked { get; set; } = false;

        // --- YENİ EKLENEN AKILLI OTURUM ALANLARI ---
        public string IpAddress { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty; // Örn: Windows PC, iPhone
        public string Browser { get; set; } = string.Empty;    // Örn: Chrome, Safari

        // --- YENİ EKLENEN AUDIT (DENETİM) ALANLARI ---
        public DateTime? RevokedDate { get; set; } // Oturum ne zaman kapatıldı?
        public DateTime LastAccessed { get; set; } // Son görülme zamanı
    }
}
