using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Entity.Enums
{
    /// <summary>
    /// Sistemdeki kullanıcıların yetki seviyelerini belirler.
    /// String yazım hatalarını önlemek için Enum kullanılmıştır.
    /// </summary>
    public enum UserRole
    {
        SuperAdmin,          // Sistemin tam hakimi, her şeyi yapabilir.
        WarehouseManager,    // Depo Müdürü (Sadece kendi deposunu yönetir).
        Staff,               // Standart Depo Personeli (Mal kabul, sayım yapar).
        SalesRepresentative  // Satış Temsilcisi (Sadece stokları görüntüleyebilir).
    }
}
