namespace MultiWarehouse.Entity.Enums.Warehouse
{
    /// <summary>
    /// Depo bloklarının fiziksel ortam şartlarını belirler.
    /// </summary>
    public enum ZoneType
    {
        General = 0,         // Normal Depolama
        ColdStorage = 1,     // Soğuk Hava Deposu (Gıda vb.)
        Controlled = 2,      // Kontrollü Ortam (Nem/Sıcaklık ayarlı, İlaç vb.)
        Hazardous = 3,       // Tehlikeli Madde (Kimyasal, Yanıcı vb.)
        Quarantine = 4,      // Karantina / Kalite Kontrol (Hasarlı, şüpheli veya onaysız ürünler)
        Returns = 5,         // İade (Müşteriden dönen ve incelenecek ürünler)
        HighValue = 6
    }
}
