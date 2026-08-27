using MultiWarehouse.Entity.Entities.Common;
using MultiWarehouse.Entity.Entities.Identity;
using MultiWarehouse.Entity.Enums.Warehouse;

namespace MultiWarehouse.Entity.Entities.Definitions
{
    /// <summary>
    /// Sistemin en tepe noktasıdır. Fiziksel depoları temsil eder (Örn: Konya Merkez Depo, İstanbul Şube).
    /// </summary>
    public class Warehouse : BaseEntity
    {
        public string Name { get; set; } = string.Empty; // Depo Adı

        // --- LOKASYON VE HARİTA BİLGİLERİ ---
        public string Country { get; set; } = "Türkiye";
        public string City { get; set; } = string.Empty;        // Örn: Konya
        public string District { get; set; } = string.Empty;    // Örn: Selçuklu
        public string FullAddress { get; set; } = string.Empty; // Örn: Akademi Mah. Yeni İstanbul Cad. No:123

        // Harita entegrasyonu için 
        public double? Latitude { get; set; }  // Enlem (Örn: 38.0242)
        public double? Longitude { get; set; } // Boylam (Örn: 32.5108)

        // --- İLETİŞİM VE YÖNETİM (YENİ EKLENEN) ---


        // Operasyonel acil durumlarda veya sevkiyatlarda aranacak iletişim numarası
        public string Phone { get; set; } = string.Empty;

        // Depodan Sorumlu Yönetici (Artık adını, soyadını ve şahsi telefonunu User tablosundan çekeceğiz)
        public Guid? ManagerId { get; set; }
        public User? Manager { get; set; }

        public double MaxCapacity { get; set; }//max kapasite

        public double UsedCapacity { get; set; }//doluluk oranı

        public WarehouseOperationalStatus OperationalStatus { get; set; } = WarehouseOperationalStatus.Active;

        // --- İLİŞKİLER

        // Bir deponun içinde birden fazla Blok (Zone) bulunur
        public List<WarehouseZone> WarehouseZones { get; set; } = new List<WarehouseZone>();
    }
}

