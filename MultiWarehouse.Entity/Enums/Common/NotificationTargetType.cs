namespace MultiWarehouse.Entity.Enums.Common
{
    /// <summary>
    /// Bildirime tıklandığında Frontend'in hangi modüle (sayfaya) yönleneceğini belirten tipler.
    /// </summary>
    public enum NotificationTargetType
    {
        None = 0,           // Herhangi bir yönlendirme yok
        Product = 1,        // Ürün detay çekmecesi
        InboundOrder = 2,   // Mal kabul detay sayfası
        OutboundOrder = 3,  // Sevkiyat detay sayfası
        TransferOrder = 4,  // Transfer detay sayfası
        Warehouse = 5,      // Depo detay sayfası
        Stock = 6,          // Stok hareketleri/detayları
        UserProfile = 7     //Kullanıcı profil ve güvenlik ayarlarına yönlendirme yapar.
    }
}