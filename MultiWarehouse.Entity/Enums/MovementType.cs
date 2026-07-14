using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Entity.Enums
{
    /// <summary>
    /// Stok hareketinin lojistik yönünü belirler.
    /// </summary>
    public enum MovementType
    {
        Inbound,  // Mal Kabul (Tedarikçiden depoya giren ürün)
        Outbound, // Sevkiyat (Depodan müşteriye veya dışarı çıkan ürün)
        Transfer  // Depolar arası veya raflar arası iç transfer
    }
}
