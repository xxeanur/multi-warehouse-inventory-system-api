using MultiWarehouse.Entity.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Entity.Entities
{
    /// <summary>
    /// Sistemi kullanacak personelleri ve yöneticileri temsil eder.
    /// Sisteme giriş (Login) ve yetkilendirme (Authorization) süreçlerinin merkezidir.
    /// </summary>
    // public class diyerek diğer katmanların (Service, API) bu sınıfa erişmesine izin veriyoruz.
    // BaseEntity'den miras alarak Id, CreatedDate, IsActive gibi ortak özellikleri otomatik dahil ediyoruz.
    public class User : BaseEntity
    {
        // Kullanıcının sistemde görünecek gerçek adını tutarız.
        public string FirstName { get; set; } = string.Empty;

        // Kullanıcının sistemde görünecek gerçek soyadını tutarız.
        public string LastName { get; set; } = string.Empty;

        // Sisteme giriş yaparken (Login) kullanıcı adı yerine geçecek olan e-posta adresi.
        // Aynı zamanda şifre sıfırlama gibi bildirimler için kullanılacak.
        public string Email { get; set; } = string.Empty;

        // Güvenlik Kuralı: Şifreler veritabanında ASLA "12345" gibi açık metin olarak tutulmaz.
        // BCrypt gibi bir algoritma ile şifrelenip karmaşık bir metin (Hash) olarak kaydedilir.
        public string PasswordHash { get; set; } = string.Empty;

        // --- GÜNCELLENEN KISIM ---
        // Artık string değil, doğrudan belirlediğimiz Enum listesinden tip güvenli (Type-Safe) bir şekilde değer alacak.
        // Sisteme yeni eklenen biri varsayılan olarak "Staff" (Personel) yetkisiyle başlar.
        public UserRole Role { get; set; } = UserRole.Staff;

        // --- AUTH & GÜVENLİK İLİŞKİLERİ ---
        public List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public List<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();

        // EKLENEN KISIM
        public string Phone { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public DateTime? LastLoginDate { get; set; }

        public bool EmailConfirmed { get; set; }

        //ilişki 
        public List<Warehouse> ManagedWarehouses { get; set; } = new();
    }
}