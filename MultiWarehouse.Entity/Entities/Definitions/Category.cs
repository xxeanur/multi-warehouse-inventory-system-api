using MultiWarehouse.Entity.Entities.Common;

namespace MultiWarehouse.Entity.Entities.Definitions
{

    public class Category : BaseEntity
    {
        public string Name { get; set; } = string.Empty; // Kategori adı
        public string Description { get; set; } = string.Empty; // Kategori açıklaması
        public List<Product> Products { get; set; } = new List<Product>();
    }
}
