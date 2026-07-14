using Microsoft.EntityFrameworkCore;
using MultiWarehouse.Entity.Entities;
using MultiWarehouse.Service.Context;
using MultiWarehouse.Service.Exceptions;
using MultiWarehouse.Service.Repositories.Interfaces;
using MultiWarehouse.Service.Services.Interfaces;
using MultiWarehouse.Shared.DTOs.AuthDtos;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MultiWarehouse.Service.Services.Implementations
{
    public class AuthService : IAuthService
    {
        // 1. FABRİKA KURULUMU (Dependency Injection)
        // Kullanılacak araçları tanımlıyoruz. Sadece kurucu metotta (constructor) değer atanabilmesi ve 
        // sonradan yanlışlıkla değiştirilip sistemin bozulmaması için 'readonly' kullanıyoruz.
        private readonly IGenericRepository<User> _userRepository;
        private readonly ITokenService _tokenService;
        private readonly AppDbContext _context;

        public AuthService(IGenericRepository<User> userRepository, ITokenService tokenService, AppDbContext context)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _context = context;
        }

        public async Task<TokenDto> LoginAsync(LoginDto loginDto)
        {
            // 2. KULLANICIYI ARAMA
            // Gelen e-posta adresiyle veritabanını sorguluyoruz. 
            // SingleOrDefaultAsync kullanıyoruz; çünkü bir e-posta sadece bir kişiye ait olmalıdır. 
            // Aynı mailden 2 kayıt çıkarsa hata fırlatıp veri bütünlüğünün bozulduğunu bize bildirir.
            var user = await _userRepository.Where(x => x.Email == loginDto.Email).SingleOrDefaultAsync();

            // 3. GÜVENLİK KONTROLLERİ (Kullanıcı var mı? Şifre doğru mu?)
            // Dikkat: Kötü niyetli kişilerin sistemde hangi maillerin kayıtlı olduğunu bulmasını 
            // (User enumeration) engellemek için her iki hatada da aynı muğlak mesajı dönüyoruz.
            if (user == null)
            {
                throw new ClientSideException("E-posta veya şifre hatalı.");
            }

            if (user.PasswordHash != loginDto.Password)
            {
                throw new ClientSideException("E-posta veya şifre hatalı.");
            }

            // 4. TOKEN ÜRETİMİ
            // Tüm kontrollerden başarıyla geçen kullanıcı için Access ve Refresh token paketi üretiyoruz.
            var tokenDto = _tokenService.CreateToken(user);

            // 5. YENİ REFRESH TOKEN NESNESİ HAZIRLAMA
            // Üretilen token'ı veritabanına (PostgreSQL) yazmak için Entity nesnesine (RefreshToken) çeviriyoruz.
            var newRefreshToken = new RefreshToken
            {
                Token = tokenDto.RefreshToken,

                // Not: PostgreSQL DateTime.Now (Yerel saat) kabul etmez, sadece UTC kabul eder. 
                // Bu yüzden TokenService içinde DateTime.UtcNow kullanılarak üretilen tarihi buraya atıyoruz.
                Expires = tokenDto.RefreshTokenExpiration,

                UserId = user.Id
            };

            // 6. VERİTABANINA EKLEME İŞLEMİ (Kritik Nokta)
            // user nesnesini komple Update etmeye çalışırsak, EF Core bu yeni token'ın eski bir kayıt 
            // olduğunu zannedip UPDATE sorgusu atar ve "DbUpdateConcurrencyException" hatası verir.
            // Bunu önlemek için açıkça "Bu yepyeni bir kayıttır, INSERT INTO yap" talimatını AddAsync ile veriyoruz.
            await _context.Set<RefreshToken>().AddAsync(newRefreshToken);

            // 7. KAYDETME (Transaction / Unit of Work)
            // Hafızada (RAM) bekleyen INSERT sorgusunu veritabanına (PostgreSQL) kalıcı olarak yazar.
            await _context.SaveChangesAsync();

            // 8. DÖNÜŞ
            // İşlem başarıyla bittiyse Controller'a saf token verisini iletiyoruz.
            // Controller da bunu CustomResponseDto içine sarıp Next.js'e 200 OK ile yollayacak.
            return tokenDto;
        }
    }
}