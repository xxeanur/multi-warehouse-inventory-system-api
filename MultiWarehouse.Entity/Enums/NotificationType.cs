using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Entity.Enums
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
        Outbound,      // Depodan mal çıkışı (sevkiyat) olduğunda
        Security       // Yetkisiz giriş denemesi veya sistem uyarıları
    }
}
