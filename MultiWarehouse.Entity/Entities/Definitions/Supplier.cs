using MultiWarehouse.Entity.Entities.Common;

namespace MultiWarehouse.Entity.Entities.Definitions
{

    public class Supplier : BaseEntity
    {
        public string CompanyName { get; set; } = string.Empty;// Tedarikçi firma adı
        public string ContactName { get; set; } = string.Empty;// İletişim kurulacak kişi
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        // --- YENİ EKLENEN KISIM ---
        // Tedarikçinin fiziksel adresi (Lojistik, fatura ve resmi evraklar için zorunludur)
        public string Country { get; set; } = "Türkiye";
        public string City { get; set; } = string.Empty;        // Örn: İstanbul
        public string District { get; set; } = string.Empty;    // Örn: Kadıköy
        public string FullAddress { get; set; } = string.Empty; // Örn: Caferağa Mah. No:4

        public double? Latitude { get; set; }  // Enlem
        public double? Longitude { get; set; } // Boylam

        // EKLENEN KISIM
        public string TaxNumber { get; set; } = string.Empty;
        public string TaxOffice { get; set; } = string.Empty;

        // ---ilişkiler

        // Bir tedarikçiden birden fazla ürün alabiliriz (One-to-Many ilişkisi)
        public List<Product> Products { get; set; } = new List<Product>();
    }
}
