using Microsoft.AspNetCore.Diagnostics;
using MultiWarehouse.Service.Exceptions;
using MultiWarehouse.Shared.DTOs;
using System.Text.Json;

namespace multi_warehouse_inventory_system_api.Middlewares
{
    // Extension (Genişletme) metodu yazabilmek için sınıfın 'static' olması zorunludur.
    public static class UseCustomExceptionHandler
    {
        // 'this IApplicationBuilder app' parametresi, bu metodun Program.cs içinde 
        // doğrudan "app.UseCustomException();" şeklinde çok temiz bir biçimde çağrılabilmesini sağlar.
        public static void UseCustomException(this IApplicationBuilder app)
        {
            // .NET'in yerleşik (built-in) global hata yakalama middleware'ini devreye sokuyoruz.
            // Projenin neresinde bir hata (Exception) patlarsa patlasın, uygulama çökmeden buraya düşer.
            app.UseExceptionHandler(config =>
            {
                // Hata anında çalışacak olan asenkron blok
                config.Run(async context =>
                {
                    // 1. İLETİŞİM FORMATI: Frontend'e hatayı çirkin bir HTML sayfası olarak değil, 
                    // rahatça işleyebileceği temiz bir JSON formatında döneceğimizi belirtiyoruz.
                    context.Response.ContentType = "application/json";

                    // 2. HATAYI YAKALAMA: Sistemin derinliklerinde fırlatılan asıl hata objesini (Exception) yakalıyoruz.
                    var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();

                    if (exceptionFeature != null)
                    {
                        // 3. STATÜ KODU BELİRLEME: Aksi ispatlanana kadar her hatayı 
                        // 500 (Internal Server Error - Sunucu/Kod Hatası) olarak kabul ediyoruz.
                        var statusCode = 500;

                        // Eğer fırlatılan hata bizim aşağıda ürettiğimiz 'ClientSideException' ise 
                        // (yani kullanıcının eksik/hatalı veri girmesi gibi iş kurallarına takılan bir hataysa) 
                        // statü kodunu 400 (Bad Request) olarak değiştiriyoruz.
                        if (exceptionFeature.Error is ClientSideException)
                        {
                            statusCode = 400;
                        }
                        // Eğer kullanıcının yetkisi olmayan bir endpoint'e veya veriye erişmeye çalışmasından 
                        // kaynaklı bir hataysa statü kodunu 401 (Unauthorized) yapıyoruz.
                        if (exceptionFeature.Error is UnauthorizedAccessException)
                        {
                            statusCode = 401;
                        }

                        // Belirlediğimiz kesin statü kodunu HTTP cevabına (Response) işliyoruz.
                        context.Response.StatusCode = statusCode;

                        // 4. STANDART DTO'YA SARMA: Frontend her zaman aynı JSON yapısını beklediği için
                        // yakalanan hata mesajını kendi oluşturduğumuz CustomResponseDto'nun içine koyuyoruz.
                        var response = CustomResponseDto.FailResponse(exceptionFeature.Error.Message);

                        // 5. JSON'A ÇEVİRME VE FIRLATMA:
                        // C# tarafındaki değişkenler BüyükHarfle (PascalCase) başlar ama JavaScript/Frontend dünyası 
                        // küçükHarf (camelCase) bekler. JsonSerializerOptions ile bu dönüşümü otomatik yapıp cevabı yolluyoruz.
                        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
                    }
                });
            });
        }
    }

    // Proje genelinde iş kurallarına uymayan durumlarda (Örn: "Stok yetersiz", "Kullanıcı bulunamadı") 
    // doğrudan 'throw new ClientSideException("mesaj")' diyerek güvenli hata fırlatmak için kullandığımız sınıf.
    // Bu sınıftan fırlatılan her hata, yukarıdaki mekanizmada anında 400 koduyla yakalanıp frontend'e iletilir.
 
}