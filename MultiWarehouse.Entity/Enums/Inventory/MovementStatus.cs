namespace MultiWarehouse.Entity.Enums.Inventory
{
    /// <summary>
    /// Stok transferlerinin ve hareketlerinin anlık durumunu belirler.
    /// </summary>
    public enum MovementStatus
    {
        Pending,   // İşlem başlatıldı hazırlanıyor
        InTransit, // Yolda (Depolar arası transferlerde)
        Completed, // Başarıyla tamamlandı ve rafa girdi
        Cancelled  // İptal edildi
    }
}
