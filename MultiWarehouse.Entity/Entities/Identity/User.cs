using MultiWarehouse.Entity.Entities.Common;
using MultiWarehouse.Entity.Entities.Definitions;
using MultiWarehouse.Entity.Enums.User;

namespace MultiWarehouse.Entity.Entities.Identity
{

    public class User : BaseEntity
    {

        public string FirstName { get; set; } = string.Empty;


        public string LastName { get; set; } = string.Empty;


        public string Email { get; set; } = string.Empty;


        public string PasswordHash { get; set; } = string.Empty;


        public UserRole Role { get; set; } = UserRole.Staff;

        public List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public List<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();

        public string Phone { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public DateTime? LastLoginDate { get; set; }

        public bool EmailConfirmed { get; set; }

        //ilişki 
        public List<Warehouse> ManagedWarehouses { get; set; } = new();

        // Personelin çalıştığı (bağlı olduğu) depo
        public Guid? WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }

        // Bildirim Tercihleri
        public bool ReceiveEmailNotifications { get; set; } = true;
        public bool ReceiveInAppNotifications { get; set; } = true;

        // E-Posta Değiştirme ve Onay Süreci
        public string? PendingNewEmail { get; set; }
        public string? EmailChangeToken { get; set; }
        public DateTime? EmailChangeTokenExpires { get; set; }
    }
}