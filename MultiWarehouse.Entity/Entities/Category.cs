using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Entity.Entities
{
    /// <summary>
    /// Ürünleri mantıksal olarak grupladığımız klasörlerdir (Örn: Elektronik, Temizlik, Gıda).
    /// Raporlama ve filtreleme süreçlerinde ürünleri kategorilerine göre kolayca ayırmamızı sağlar.
    /// </summary>
    public class Category : BaseEntity
    {
        public string Name { get; set; } = string.Empty; // Kategori adı
        public string Description { get; set; } = string.Empty; // Kategori açıklaması

        // Bir kategorinin altında birden fazla ürün bulunabilir (One-to-Many ilişkisi)
        public List<Product> Products { get; set; } = new List<Product>();
    }
}
