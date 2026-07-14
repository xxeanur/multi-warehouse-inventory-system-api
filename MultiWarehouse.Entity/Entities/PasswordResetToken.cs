using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Entity.Entities
{
    /// <summary>
    /// Kullanıcı "Şifremi Unuttum" dediğinde maile gidecek olan güvenli, süreli token bilgisini tutar.
    /// </summary>
    public class PasswordResetToken : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public string Token { get; set; } = string.Empty; // URL'de gidecek olan benzersiz şifrelenmiş metin

        public DateTime ExpireDate { get; set; } // Token'ın son kullanma tarihi (Örn: 2 saat geçerli)

        public bool IsUsed { get; set; } = false; // Token kullanıldı mı? (Tek kullanımlık olmasını sağlar)
    }
}
