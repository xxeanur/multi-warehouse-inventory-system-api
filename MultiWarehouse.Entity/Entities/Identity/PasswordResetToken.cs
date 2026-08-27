using MultiWarehouse.Entity.Entities.Common;

namespace MultiWarehouse.Entity.Entities.Identity
{
    /// <summary>
    /// Kullanıcı "Şifremi Unuttum" dediğinde maile gidecek olan güvenli, süreli token bilgisini tutar.
    /// </summary>
    public class PasswordResetToken : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public string Token { get; set; } = string.Empty;

        public DateTime ExpireDate { get; set; }

        public bool IsUsed { get; set; } = false;
    }
}
