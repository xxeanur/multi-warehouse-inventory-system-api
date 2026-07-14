using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Entity.Enums
{
    /// <summary>
    /// Depo sayım işleminin anlık durumunu tutar.
    /// </summary>
    public enum CountStatus
    {
        Planned,    // Sayım planlandı ama henüz başlamadı
        InProgress, // Personel şu an sayım yapıyor
        Completed,  // Sayım bitti ve onaylandı
        Cancelled   // Sayım iptal edildi
    }
}
