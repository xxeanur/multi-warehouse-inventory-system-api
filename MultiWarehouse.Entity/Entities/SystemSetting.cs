using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Entity.Entities
{
    /// <summary>
    /// Sistemin esnek ve dinamik kalmasını sağlayan genel ayarlar tablosudur.
    /// Ayarları koda gömmek yerine (Hardcode), buradan anahtar-değer şeklinde okuruz.
    /// </summary>
    public class SystemSetting : BaseEntity
    {
        // Örn: "MaxFileUploadSize", "DefaultCurrency"
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
