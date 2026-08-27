namespace MultiWarehouse.Entity.Enums.Common
{
    /// <summary>
    /// Bildirimlerin kategorisini belirler. 
    /// Frontend tarafında ikon ve renk yönetimi için kullanılır.
    /// </summary>
    public enum NotificationType
    {
        CriticalStock, // Stok kritik seviyenin altına düştüğünde
        Transfer,      // Depolar veya raflar arası ürün transfer edildiğinde
        Inbound,       // Depoya yeni mal girişi olduğunda
        Outbound,      // Depodan mal çıkışı olduğunda
        Security       // Yetkisiz giriş denemesi veya sistem uyarıları
    }
}
