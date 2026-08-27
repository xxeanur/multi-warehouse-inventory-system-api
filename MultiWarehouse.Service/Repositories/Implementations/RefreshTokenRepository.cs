using Microsoft.EntityFrameworkCore;
using MultiWarehouse.Entity.Entities.Identity;
using MultiWarehouse.Service.Context;
using MultiWarehouse.Service.Repositories.Interfaces;

namespace MultiWarehouse.Service.Repositories.Implementations
{
    public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(AppDbContext context) : base(context)
        {
        }

        #region Read Operations

        public async Task<RefreshToken?> GetByTokenWithUserAsync(string token)
        {
            return await _context.Set<RefreshToken>()
                .Include(x => x.User)
                .SingleOrDefaultAsync(x => x.Token == token);
        }

        #endregion
    }
}