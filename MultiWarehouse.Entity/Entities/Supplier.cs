using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Entity.Entities
{
    /// <summary>
    /// Depoya ürün sağlayan toptancı veya üretici firmaları (Tedarikçileri) temsil eder.
    /// Ürünlerin kaynağını takip etmek için kullanılır.
    /// </summary>
    public class Supplier : BaseEntity
    {
        public string CompanyName { get; set; } = string.Empty;// Tedarikçi firma adı
        public string ContactName { get; set; } = string.Empty;// İletişim kurulacak kişi
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        // --- YENİ EKLENEN KISIM ---
        // Tedarikçinin fiziksel adresi (Lojistik, fatura ve resmi evraklar için zorunludur)
        public string Address { get; set; } = string.Empty;

        // EKLENEN KISIM
        public string TaxNumber { get; set; } = string.Empty;
        public string TaxOffice { get; set; } = string.Empty;

        // --- İLİŞKİLER (FOREIGN KEYS) ---

        // Bir tedarikçiden birden fazla ürün alabiliriz (One-to-Many ilişkisi)
        public List<Product> Products { get; set; } = new List<Product>();
    }
}
