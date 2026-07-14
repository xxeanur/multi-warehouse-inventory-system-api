using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Entity.Entities
{
    /// <summary>
    /// Kullanıcının sürekli login olmasını engellemek için arka planda JWT yenilemeye yarayan uzun ömürlü anahtardır.
    /// </summary>
    public class RefreshToken : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public string Token { get; set; } = string.Empty;

        public DateTime Expires { get; set; } // Genelde 7 gün veya 1 ay gibi uzun bir süre verilir

        // Güvenlik: Admin bu token'ı iptal etti mi? (Eğer true ise, kullanıcı yeni JWT alamaz ve sistemden atılır)
        public bool IsRevoked { get; set; } = false;
    }
}
