using Microsoft.EntityFrameworkCore;
using MultiWarehouse.Entity.Entities.Identity;
using MultiWarehouse.Service.Context;
using MultiWarehouse.Service.Repositories.Interfaces;

namespace MultiWarehouse.Service.Repositories.Implementations
{
    public class PasswordResetTokenRepository : GenericRepository<PasswordResetToken>, IPasswordResetTokenRepository
    {
        public PasswordResetTokenRepository(AppDbContext context) : base(context)
        {
        }

        #region Read Operations

        public async Task<PasswordResetToken?> GetByTokenWithUserAsync(string token)
        {
            return await _context.Set<PasswordResetToken>()
                .Include(x => x.User)
                .SingleOrDefaultAsync(x => x.Token == token);
        }

        #endregion
    }
}