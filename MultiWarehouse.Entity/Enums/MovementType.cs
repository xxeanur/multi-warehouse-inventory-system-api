// MultiWarehouse.Entity/Enums/MovementType.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Entity.Enums
{
    /// <summary>
    /// Stok hareketinin lojistik yönünü ve operasyon türünü belirler.
    /// </summary>
    public enum MovementType
    {
        // GİRİŞ İŞLEMLERİ
        Inbound = 1,           // Mal Kabul (Tedarikçiden depoya giren sıfır ürün)
        CustomerReturn = 2,    // Müşteri İadesi (Satılmış ürünün depoya geri dönmesi)

        // ÇIKIŞ İŞLEMLERİ
        Outbound = 3,          // Sevkiyat / Satış (Depodan müşteriye çıkış)
        SupplierReturn = 4,    // Tedarikçi İadesi (Kusurlu/yanlış gelen malın fabrikaya geri gönderilmesi)
        Scrap = 5,             // Fire / Hurda (Kırılan, bozulan veya miadı dolan ürünün stoktan düşülmesi)

        // TRANSFER İŞLEMLERİ
        WarehouseTransfer = 6, // Depolar Arası Transfer (Örn: Konya Merkez Depo'dan -> Ankara Şube Depo'ya)
        ShelfTransfer = 7,     // Raflar Arası İç Transfer (Aynı deponun içinde A-01 rafından B-02 rafına taşıma)

        // DÜZELTME İŞLEMLERİ
        Adjustment = 8         // Sayım Düzeltmesi (Yıl sonu/ay sonu sayımlarında çıkan eksik veya fazlalıkları eşitlemek için atılan sanal hareket)
    }
}