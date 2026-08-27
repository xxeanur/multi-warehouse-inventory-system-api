namespace MultiWarehouse.Entity.Enums.User
{
    /// <summary>
    /// Sistemdeki kullanıcıların yetki seviyelerini belirler.
    /// String yazım hatalarını önlemek için Enum kullanılmıştır.
    /// </summary>
    public enum UserRole
    {
        SuperAdmin,          // Sistemin tam hakimi, her şeyi yapabilir.
        WarehouseManager,    // Depo Müdürü (Sadece kendi deposunu yönetir).
        Staff,               // Standart Depo Personeli (Mal kabul, sayım yapar).
    }
}
