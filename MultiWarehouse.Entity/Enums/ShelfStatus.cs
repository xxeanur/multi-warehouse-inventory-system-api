using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Entity.Enums
{
    /// <summary>
    /// Depodaki rafların operasyonel uygunluğunu belirler.
    /// </summary>
    public enum ShelfStatus
    {
        Available,   // Kullanıma hazır, boş yer varsa ürün konulabilir
        Maintenance, // Hasarlı veya bakımda, işlem yapılamaz
        Reserved     // Gelecek bir mal kabul için önceden ayrılmış
    }
}
