namespace MultiWarehouse.Entity.Enums.Inventory
{
    public enum CountStatus
    {
        Matched = 1,  // Eşleşti (Sistem = Sayılan)
        Shortage = 2, // Eksik (Sayılan < Sistem)
        Overage = 3   // Fazla (Sayılan > Sistem)
    }
}