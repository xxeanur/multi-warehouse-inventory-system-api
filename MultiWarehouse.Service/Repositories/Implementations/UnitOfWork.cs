using Microsoft.EntityFrameworkCore.Storage;
using MultiWarehouse.Service.Context;
using MultiWarehouse.Service.Repositories.Interfaces;

namespace MultiWarehouse.Service.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _currentTransaction;

        public UnitOfWork(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region Save Operations

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        #endregion

        #region Transaction Operations

        public async Task BeginTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                throw new InvalidOperationException("Halihazırda devam eden bir transaction mevcut.");
            }

            _currentTransaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                if (_currentTransaction == null)
                    throw new InvalidOperationException("Onaylanacak aktif bir transaction bulunamadı.");

                await _currentTransaction.CommitAsync();
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            try
            {
                if (_currentTransaction == null)
                    throw new InvalidOperationException("Geri alınacak aktif bir transaction bulunamadı.");

                await _currentTransaction.RollbackAsync();
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            _currentTransaction?.Dispose();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}