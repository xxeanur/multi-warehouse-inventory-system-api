namespace MultiWarehouse.Service.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// EF Core tarafından izlenen (Track edilen) tüm değişiklikleri veritabanına yansıtır.
        /// EF Core kendi içinde bu işlemi implicit (gizli) bir transaction ile yapar.
        /// </summary>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Manuel bir veritabanı transaction'ı başlatır. 
        /// Birden fazla SaveChangesAsync() çağrılacaksa ve hata anında tüm işlemlerin geri alınması (Rollback) isteniyorsa kullanılır.
        /// </summary>
        Task BeginTransactionAsync();

        /// <summary>
        /// Başlatılan transaction'ı onaylar ve veritabanına kalıcı olarak yazar.
        /// </summary>
        Task CommitTransactionAsync();

        /// <summary>
        /// Başlatılan transaction sırasında bir hata oluşursa, yapılan tüm değişiklikleri geri alır.
        /// </summary>
        Task RollbackTransactionAsync();
    }
}