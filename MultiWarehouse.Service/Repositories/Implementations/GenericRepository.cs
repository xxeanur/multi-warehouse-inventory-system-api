using Microsoft.EntityFrameworkCore;
using MultiWarehouse.Service.Context;
using MultiWarehouse.Service.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Service.Repositories.Implementations
{
    // <T> (Generic Yapı): Bu sınıfın belirli bir tabloya bağımlı olmadığını gösterir. T burada bir şablondur.
    // where T : class: Sisteme, "Bu T yerine sadece veritabanı tablolarımızı temsil eden referans tipli sınıfları (Entity'leri) gönderebilirsin" diyerek tip güvenliğini (Type Safety) sağlarız.
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        // _context: Veritabanıyla olan ana bağlantımızdır. protected yapmamızın sebebi, yarın öbür gün bu sınıftan miras alan özel bir repo (Örn: ProductRepository) yazmak istersek, onun da bu bağlantıya erişebilmesini sağlamaktır.
        protected readonly AppDbContext _context;

        // _dbSet: Hedef tabloyu (örneğin Products tablosunu) hafızaya aldığımız ve işlemleri doğrudan üzerinden yaptığımız değişkendir.
        private readonly DbSet<T> _dbSet;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        /// <summary>
        /// Veritabanındaki tek bir kaydı ID'sine göre getirir.
        /// </summary>
        public async Task<T?> GetByIdAsync(Guid id)
        {
            // FindAsync: EF Core'un en performanslı ID arama metodudur. Önce veritabanına gitmek yerine bellekte (Local Cache) bu ID'ye sahip kayıt var mı diye bakar. Bulamazsa SQL sorgusu atar.
            return await _dbSet.FindAsync(id);
        }

        /// <summary>
        /// Tablodaki tüm kayıtları getirir.
        /// </summary>
        public IQueryable<T> GetAll()
        {
            // IQueryable: Veriyi veritabanından hemen çekmez (Deferred Execution). Sonuna .ToList() veya .FirstOrDefault() diyene kadar SQL çalışmaz. Bu sayede arkasına .Take(10) ekleyip sayfalama (Pagination) yapılabilir.
            // AsNoTracking: "Bu veriyi sadece okuyacağım, üzerinde güncelleme yapmayacağım" demektir. EF Core'un her satırı takip etmesini engeller ve RAM kullanımını muazzam iyileştirir.
            return _dbSet.AsNoTracking();
        }

        /// <summary>
        /// Belirli bir şarta uyan kayıtları getirir.
        /// </summary>
        public IQueryable<T> Where(Expression<Func<T, bool>> expression)
        {
            // Expression<Func<T, bool>>: Yazdığımız LINQ şartlarını (Örn: x => x.IsActive == true) arka planda güvenli bir şekilde ham SQL WHERE sorgusuna çevirir.
            return _dbSet.Where(expression);
        }

        /// <summary>
        /// Belirli bir şarta uyan tek bir kayıt bile var mı diye kontrol eder.
        /// </summary>
        public async Task<bool> AnyAsync(Expression<Func<T, bool>> expression)
        {
            // Bütün veriyi RAM'e çekmek yerine, veritabanında şarta uyan kayıt var mı diye bakar ve true/false döner. "SELECT EXISTS(...)" şeklinde çok hafif bir SQL üretir.
            return await _dbSet.AnyAsync(expression);
        }

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
            // Not: Update ve Remove işlemleri asenkron (Async) değildir. 
            // Çünkü o anda veritabanına gidip bir şey silmez/güncellemezler. 
            // Sadece nesnenin EF Core belleğindeki durumunu (State) Modified veya Deleted olarak işaretlerler.
            _dbSet.Update(entity);
        }

        /// <summary>
        /// Var olan bir kaydı silinmek üzere işaretler.
        /// </summary>
        public void Remove(T entity)
        {
            // Asıl işlem (SQL'e yansıma), servis katmanında tüm işler bitip "await _context.SaveChangesAsync()" çağrıldığında gerçekleşir.
            _dbSet.Remove(entity);
        }
    }
}