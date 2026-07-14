using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Entity.Enums
{
    /// <summary>
    /// Depo bloklarının fiziksel ortam şartlarını belirler.
    /// </summary>
    public enum ZoneType
    {
        General,      // Standart, oda sıcaklığında genel ürünler
        ColdStorage,  // Soğuk hava deposu (Gıda, İlaç)
        Electronics,  // Nemden arındırılmış, statik elektriğe karşı korumalı alan
        Chemical      // Yanıcı, parlayıcı veya tehlikeli madde alanı
    }
}
