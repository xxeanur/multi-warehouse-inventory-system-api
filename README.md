# 🏢 Multi-Warehouse Management System (WMS) - RESTful API

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12-239120?style=for-the-badge&logo=c-sharp)
![EF Core](https://img.shields.io/badge/Entity_Framework_Core-Code_First-339933?style=for-the-badge)
![SQL Server](https://img.shields.io/badge/MS_SQL_Server-Latest-CC2927?style=for-the-badge&logo=microsoft-sql-server)

Bu proje, çoklu depo (Multi-Tenancy) mimarisine sahip işletmelerin lojistik ve stok operasyonlarını tek merkezden, güvenli ve yüksek performanslı bir şekilde yönetmek amacıyla tasarlanmış **Kurumsal Seviye (Enterprise-grade) bir Depo Yönetim Sistemi Backend API** uygulamasıdır.

Sistem, dışarıdan serbest kayıtlara kapalı (**No Public Register**), tamamen kapalı devre (**Closed-loop**) bir yetkilendirme mimarisiyle çalışır. Personel ve yöneticiler, sadece sistem yöneticileri (SuperAdmin) veya ilgili depo müdürleri tarafından sisteme dahil edilebilir.

---

## 🛠️ Mimari ve Kullanılan Teknolojiler

Proje, **Onion Architecture (Soğan Mimarisi)** ve **Domain-Driven Design (DDD)** prensipleri benimsenerek katmanlı bir yapıda geliştirilmiştir. Kontrolcüler (Controllers) veri erişiminden izole edilmiş, iş kuralları servis katmanına taşınmıştır.

- **Framework & Dil:** .NET 8 / ASP.NET Core Web API, C# 12
- **Veritabanı & ORM:** MS SQL Server & **Entity Framework Core** (Code-First Yaklaşımı)
- **Mimari Kalıplar:** N-Tier Architecture, Generic Repository Pattern, Unit of Work (Transaction Yönetimi)
- **Veri Doğrulama (Validation):** **FluentValidation** (İş kuralları ve veri bütünlüğü Controller dışında, middleware/pipeline seviyesinde denetlenir)
- **Kimlik Doğrulama:** Custom JWT (JSON Web Token), Refresh Token, BCrypt Hashing
- **Harici Kütüphaneler:** 
  - `AutoMapper`: Entity - DTO arası hızlı ve güvenli haritalama.
  - `MailKit`: Asenkron ve güvenli SMTP e-posta gönderimi.
  - `UAParser`: Denetim izleri (Audit Logging) için cihaz ve tarayıcı analizi.

---

## 🔒 Güvenlik ve Veri Bütünlüğü (Security & Data Integrity)

- **Row-Level Security (RLS):** Personeller ve Depo Müdürleri, API üzerinden sadece **kendi atandıkları depolara** ait verileri görebilir ve işlem yapabilirler. Bu izolasyon, JWT içerisindeki `WarehouseId` claim'i üzerinden servis katmanında (IDOR Koruması ile) güvence altındadır.
- **FluentValidation Entegrasyonu:** Tüm DTO'lar için yazılmış kurallar (Örn: Hacim eksi olamaz, miktar sıfırdan büyük olmalıdır) Controller'a ulaşmadan araya girerek (ActionFilter/Middleware) geçersiz istekleri anında reddeder. Temiz ve güvenli veri akışı sağlar.
- **Bağımlılık Kontrolü (Soft Delete):** 
  - İçerisinde aktif fiziksel stok bulunan hiçbir ürün silinemez.
  - İçerisinde aktif raf (Shelf) barındıran hiçbir depo alanı (Zone) silinemez. Veritabanından kalıcı veri silinmez, `IsActive = false` (Soft Delete) mantığı ile pasife çekilir.

---

## 🧩 Modüller ve API Uç Noktaları (Controllers)

Sistemdeki **19 Controller**, iş alanlarına (Domain) göre 5 ana modüle ayrılmıştır:

### 1. Kimlik, Kullanıcı ve Güvenlik Yönetimi (Identity)
- **`AuthController`:** Login, Refresh Token yenileme, Logout ve yetkisiz cihazların oturumlarını sonlandırma (Revoke). Dışarıya açık kayıt (Register) yoktur.
- **`UsersController`:** Sadece yetkililerin personel ekleyebildiği, hesap durumlarını (Aktif/Pasif) yönetebildiği ve güvenli e-posta/şifre değiştirme operasyonlarının yürütüldüğü merkez.
- **`AuditLogsController`:** Sistemin kara kutusu. Hangi kullanıcının hangi tabloda hangi veriyi (eski ve yeni JSON formatında) değiştirdiğini kayıt altına alır. (Sadece SuperAdmin okuyabilir).

### 2. Ana Veri ve Tanımlamalar (Master Data)
- **`ProductsController`:** Ürün CRUD işlemleri. Barkod, SKU ve metin tabanlı (Search) arama desteği.
- **`CategoriesController` & `SuppliersController`:** Kategori ve tedarikçi tanımlamaları.
- **`WarehousesController`, `WarehouseZonesController`, `ShelvesController`:** Depo, Blok/Alan (Zone) ve Raf hiyerarşisinin yönetimi. Rafların hacim (cm³) ve ağırlık (kg) bazlı anlık doluluk oranları burada takip edilir.

### 3. Belge ve Operasyon Yönetimi (Documents)
Tüm belge akışları `Pending -> InTransit / Approved -> Completed` durum makineleriyle (State Machine) yönetilir.
- **`InboundOrdersController` (Mal Kabul):** Tedarikçiden gelen malların kapıda sayılıp onaylandığı giriş fişleri.
- **`OutboundOrdersController` (Sevkiyat):** Müşteriye çıkacak ürünler için hedef raflardan stokların **rezerve** edilerek (Reserved Quantity) oluşturulduğu çıkış fişleri.
- **`TransferOrdersController`:** İki depo arasındaki stok transferleri (Yola Çıktı / Teslim Alındı).

### 4. Lojistik ve Stok Operasyonları (Inventory)
- **`PutawayController` (Raflama):** Mal kabulü biten ürünlerin fiziksel raflara dizilmesi (Execute) ve raf kapasitelerinin hesaplanması.
- **`InventoryCountController` (Akıllı Sayım):** Sistem stoğu ile fiziksel (kapı) sayımını kıyaslayan modül. **Eşleşti (Matched)**, **Eksik (Shortage)** veya **Fazla (Overage)** varyans algoritmaları ile farklılık tespit edilirse otomatik düzeltme (Adjustment) hareketleri oluşturur.
- **`StocksController`:** Ürün, Depo ve Raf bazında sistemdeki anlık "Kullanılabilir Miktar" ve "Rezerve Miktar" sorguları.
- **`StockMovementsController`:** Sistemdeki tüm giriş, çıkış, fire ve transfer işlemlerinin hareket defteri (Audit History).

### 5. Sistem Araçları (Utilities)
- **`DashboardController`:** Kokpit ekranı için sistem özetlerini (kartlar, grafik verileri) tek bir response içinde sunar.
- **`NotificationsController`:** Sistem içi kritik operasyon uyarılarını (Örn: Sayımda eksik çıktı) personele iletir.
- **`SearchController`:** WMS içerisinde kelime bazlı Global Arama (Cross-module arama) yapar.

---

## ⚙️ Kurulum (Getting Started)

Projenin yerel ortamınızda çalıştırılabilmesi için aşağıdaki adımları izleyin:

### 1. Depoyu Klonlayın
```bash
git clone <repo-url>
cd <backend-folder>
2. Veritabanı (appsettings) Ayarları
appsettings.json dosyası GitHub'da şablon olarak bulunmaktadır. Kendi bilgisayarınızda bir appsettings.Development.json dosyası oluşturun ve gizli bilgilerinizi ekleyin:

JSON
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=MultiWarehouseDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  },
  "CustomTokenOption": {
    "SecurityKey": "super_gizli_ve_uzun_jwt_anahtariniz"
  }
}
3. Migrasyonlar (Entity Framework)
Bağımlılıkları yükleyin ve Code-First veritabanını oluşturun:

Bash
dotnet restore
dotnet ef database update --project MultiWarehouse.Service --startup-project MultiWarehouse.API
4. Projeyi Çalıştırın
Bash
dotnet run --project MultiWarehouse.API
API başarıyla ayağa kalktığında https://localhost:<port>/swagger adresine giderek tüm endpoint'leri inceleyebilir ve Authorize butonuyla JWT token'ınızı girerek uç noktaları test edebilirsiniz.
