using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Entity.Entities
{
    public abstract class BaseEntity
    {
        // Tüm tablolarda standart olarak bulunacak ID
        public Guid Id { get; set; } = Guid.NewGuid();

        // Denetim (Audit) Alanları: Kaydın ne zaman oluşturulduğu ve güncellendiği
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        // Soft Delete: Veriyi veritabanından tamamen uçurmak (Drop/Delete) tehlikelidir. 
        // Silinme durumunda bu değeri 'false' yaparak veriyi pasife çeker, görünmez yaparız.
        public bool IsActive { get; set; } = true;
    }
}
