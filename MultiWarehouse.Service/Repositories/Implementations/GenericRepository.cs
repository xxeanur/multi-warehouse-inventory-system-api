using Microsoft.EntityFrameworkCore;
using MultiWarehouse.Service.Context;
using MultiWarehouse.Service.Repositories.Interfaces;
using MultiWarehouse.Shared.Pagination;
using System.Linq.Expressions;

namespace MultiWarehouse.Service.Repositories.Implementations
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        #region Read Operations

        /// <summary>
        /// Veritabanındaki tek bir kaydı ID'sine göre getirir.
        /// </summary>
        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        /// <summary>
        /// Tablodaki tüm kayıtları AsNoTracking ile takip edilmeden getirir.
        /// </summary>
        public IQueryable<T> GetAll()
        {
            return _dbSet.AsNoTracking();
        }

        /// <summary>
        /// Belirli bir şarta uyan kayıtları getirir.
        /// </summary>
        public IQueryable<T> Where(Expression<Func<T, bool>> expression)
        {
            return _dbSet.Where(expression);
        }

        /// <summary>
        /// Belirli bir şarta uyan kayıt var mı kontrol eder.
        /// </summary>
        public async Task<bool> AnyAsync(Expression<Func<T, bool>> expression)
        {
            return await _dbSet.AnyAsync(expression);
        }

        /// <summary>
        /// Filtreleme, include ve sıralama özellikleri ile sayfalanmış veri döner.
        /// </summary>
        public async Task<PagedResult<T>> GetPagedAsync(
            PaginationParams paginationParams,
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IQueryable<T>>? include = null)
        {
            IQueryable<T> query = _dbSet;

            if (filter != null) query = query.Where(filter);
            if (include != null) query = include(query);
            if (orderBy != null) query = orderBy(query);

            var totalRecords = await query.CountAsync();

            var data = await query
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            return new PagedResult<T>(data, totalRecords, paginationParams.PageNumber, paginationParams.PageSize);
        }

        #endregion

        #region Write Operations

        /// <summary>
        /// Yeni bir kaydı veritabanına eklenmek üzere işaretler.
        /// </summary>
        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        /// <summary>
        /// Var olan bir kaydı güncellenmek üzere işaretler.
        /// </summary>
        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        /// <summary>
        /// Var olan bir kaydı silinmek üzere işaretler.
        /// </summary>
        public void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }

        #endregion
    }
}