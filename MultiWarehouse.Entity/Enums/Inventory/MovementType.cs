namespace MultiWarehouse.Entity.Enums.Inventory
{
    /// <summary>
    /// Stok hareketinin lojistik yönünü ve operasyon türünü belirler.
    /// </summary>
    public enum MovementType
    {
        // GİRİŞ İŞLEMLERİ
        Inbound = 1,           // Tedarikçiden depoya ürün girişi (Mal Kabul).
        CustomerReturn = 2,    // Müşteriden dönen ürünün depoya girişi.

        // ÇIKIŞ İŞLEMLERİ
        Outbound = 3,          // Depodan müşteriye ürün çıkışı (Satış/Sevkiyat).
        SupplierReturn = 4,    // Kusurlu veya yanlış ürünün tedarikçiye iade edilmesi.
        Scrap = 5,             // Hasarlı veya tarihi geçmiş ürünün stoktan düşülmesi (Fire).

        // TRANSFER İŞLEMLERİ
        TransferIn = 6,        // Başka bir depodan bu depoya ürün girmesi.
        TransferOut = 7,       // Bu depodan başka bir depoya ürün çıkması.
        ShelfTransfer = 8,     // Aynı depo içinde ürünün başka bir rafa taşınması.

        // DÜZELTME İŞLEMLERİ
        AdjustmentIn = 9,      // Sayım sonucu çıkan stok fazlasının sisteme eklenmesi.
        Reversal = 10,         // Hatalı işlemin iptal edilmesi (Ters kayıt).
        AdjustmentOut = 11     // Sayım sonucu çıkan stok eksiğinin sistemden düşülmesi.
    }
}