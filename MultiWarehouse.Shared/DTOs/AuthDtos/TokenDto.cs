using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Shared.DTOs.AuthDtos
{//oluşturduğum JWT frontende gidiyor.
    public class TokenDto
    {
        // API'ye istek atarken kullanılacak asıl yetki anahtarı
        public string AccessToken { get; set; } = string.Empty;

        // Access token'ın süresi bittiğinde, kullanıcıyı tekrar giriş ekranına 
        // atmadan arka planda yeni token almak için kullanılacak uzun ömürlü anahtar
        public string RefreshToken { get; set; } = string.Empty;

        // Frontend tarafında "Token'ın süresi ne zaman bitiyor?" kontrolü yapabilmek için
        public DateTime AccessTokenExpiration { get; set; }

        public DateTime RefreshTokenExpiration { get; set; }
    }
}
