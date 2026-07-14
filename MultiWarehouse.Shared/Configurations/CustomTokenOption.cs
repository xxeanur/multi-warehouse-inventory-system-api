using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Shared.Configurations
{
    public class CustomTokenOption
    {
        // JSON'daki property isimleriyle BİREBİR aynı olmak zorundadır.
        // Aksi takdirde eşleşme (Binding) başarısız olur ve değerler null gelir.

        public string Audience { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string SecurityKey { get; set; } = string.Empty;

        // JSON'da bunları sayı olarak verdiğimiz için C# tarafında 'int' olarak karşılıyoruz.
        public int AccessTokenExpiration { get; set; }
        public int RefreshTokenExpiration { get; set; }
    }
}
