namespace MultiWarehouse.Entity.Enums.Warehouse
{
    /// <summary>
    /// Depodaki rafların operasyonel uygunluğunu belirler.
    /// </summary>
    public enum ShelfStatus
    {
        Available,   // Kullanıma hazır, boş yer varsa ürün konulabilir
        Maintenance, // Hasarlı veya bakımda, işlem yapılamaz
        Reserved     // Gelecek bir mal kabul için önceden ayrılmış
    }
}
