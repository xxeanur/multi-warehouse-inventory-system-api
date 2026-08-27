namespace MultiWarehouse.Entity.Entities.Common
{
    public abstract class BaseEntity
    {
        // Tüm tablolarda standart olarak bulunacak ID
        public Guid Id { get; set; } = Guid.NewGuid();

        // Denetim (Audit) Alanları: Kaydın ne zaman oluşturulduğu ve güncellendiği
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        // Soft delete
        public bool IsActive { get; set; } = true;
    }
}
