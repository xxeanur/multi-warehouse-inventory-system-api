using Microsoft.EntityFrameworkCore;
using MultiWarehouse.Entity.Entities;
using MultiWarehouse.Service.Context;
using MultiWarehouse.Service.Repositories.Interfaces;
using System.Threading.Tasks;

namespace MultiWarehouse.Service.Repositories.Implementations
{
    public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
    {
        // Base sınıfın (GenericRepository) DbContext'e ihtiyacı var, onu aşağıya iletiyoruz
        public RefreshTokenRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<RefreshToken?> GetByTokenWithUserAsync(string token)
        {
            // İşte o karmaşık Include işlemi artık servis katmanında değil, 
            // ait olduğu yerde, veri erişim (Repository) katmanında yapılıyor!
            return await _context.Set<RefreshToken>()
                .Include(x => x.User)
                .SingleOrDefaultAsync(x => x.Token == token);
        }
    }
}