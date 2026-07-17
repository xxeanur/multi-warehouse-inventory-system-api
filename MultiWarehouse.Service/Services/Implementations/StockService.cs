// MultiWarehouse.Service/Services/Implementations/StockService.cs
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MultiWarehouse.Entity.Entities;
using MultiWarehouse.Service.Context;
using MultiWarehouse.Service.Exceptions;
using MultiWarehouse.Service.Repositories.Interfaces;
using MultiWarehouse.Service.Services.Interfaces;
using MultiWarehouse.Shared.DTOs.StockDtos;
using MultiWarehouse.Shared.Pagination;

namespace MultiWarehouse.Service.Services.Implementations
{
    /// <summary>
    /// Depo içerisindeki stok hareketlerini (giriş, çıkış, lokasyon değişimi) ve kapasite yönetimini üstlenen servis sınıfı.
    /// İşlemler sırasında fiziksel kuralları (hacim, ağırlık, lokasyon eşleşmesi) denetler ve veri bütünlüğünü sağlamak için Transaction kullanır.
    /// </summary>
    public class StockService : IStockService
    {
        private readonly IGenericRepository<Stock> _stockRepository;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public StockService(IGenericRepository<Stock> stockRepository, AppDbContext context, IMapper mapper)
        {
            _stockRepository = stockRepository;
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Sisteme yeni bir stok kaydı ekler. Eklenen ürünün hacim ve ağırlığına göre ilgili rafın ve deponun doluluk oranlarını günceller.
        /// </summary>
        public async Task<StockDto> CreateAsync(StockCreateDto createDto)
        {
            // İş kuralı: Rezerve edilen miktar, toplam miktarı aşamaz.
            ValidateReservedQuantity(createDto.Quantity, createDto.ReservedQuantity);

            // Veritabanı işlemleri başlar. Herhangi bir hata olursa tüm değişiklikler (kapasite güncellemeleri dahil) geri alınır.
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. İlişkili verilerin (Product, Warehouse, Shelf) veritabanında aktif olarak var olup olmadığı kontrol edilir.
                var product = await GetActiveProductAsync(createDto.ProductId);
                var warehouse = await GetActiveWarehouseAsync(createDto.WarehouseId);
                var shelf = await GetActiveShelfWithZoneAsync(createDto.ShelfId, createDto.WarehouseId);

                // 2. Aynı rafta bu üründen zaten bir satır var mı kontrolü (Aynı üründen iki satır olamaz, miktar güncellenmelidir).
                await CheckDuplicateStockAsync(createDto.ProductId, createDto.ShelfId);

                // 3. Rafın ve deponun mevcut kapasiteleri, eklenecek ürünün hacmi/ağırlığı kadar artırılır. Limitler aşılırsa hata fırlatılır.
                IncreaseCapacity(shelf, warehouse, product, createDto.Quantity);

                // 4. Stok kaydı oluşturulur ve hareket tarihi atanır.
                var stock = _mapper.Map<Stock>(createDto);
                stock.LastMovementDate = DateTime.UtcNow;

                await _stockRepository.AddAsync(stock);
                await _context.SaveChangesAsync();

                // Tüm adımlar başarılıysa işlemler veritabanına kalıcı olarak işlenir.
                await transaction.CommitAsync();

                return _mapper.Map<StockDto>(stock);
            }
            catch
            {
                // Hata durumunda veritabanı işlemlerinin tamamı iptal edilir (Veri tutarsızlığı önlenir).
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Belirtilen ID'ye sahip aktif stok kaydını getirir.
        /// </summary>
        public async Task<StockDto> GetByIdAsync(Guid id)
        {
            var stock = await _stockRepository.Where(s => s.Id == id && s.IsActive).SingleOrDefaultAsync();
            if (stock == null) throw new ClientSideException("Stok kaydı bulunamadı.");
            return _mapper.Map<StockDto>(stock);
        }

        /// <summary>
        /// Sistemdeki tüm aktif stokları listeler.
        /// </summary>
        public async Task<IEnumerable<StockDto>> GetAllAsync()
        {
            var stocks = await _stockRepository.Where(s => s.IsActive).ToListAsync();
            return _mapper.Map<IEnumerable<StockDto>>(stocks);
        }

        /// <summary>
        /// Belirli bir ürüne ait tüm lokasyonlardaki (depo/raf) stok kayıtlarını listeler.
        /// </summary>
        public async Task<IEnumerable<StockDto>> GetAllByProductIdAsync(Guid productId)
        {
            var stocks = await _stockRepository.Where(s => s.ProductId == productId && s.IsActive).ToListAsync();
            return _mapper.Map<IEnumerable<StockDto>>(stocks);
        }

        /// <summary>
        /// Belirli bir deponun içindeki tüm stokları listeler.
        /// </summary>
        public async Task<IEnumerable<StockDto>> GetAllByWarehouseIdAsync(Guid warehouseId)
        {
            var stocks = await _stockRepository.Where(s => s.WarehouseId == warehouseId && s.IsActive).ToListAsync();
            return _mapper.Map<IEnumerable<StockDto>>(stocks);
        }

        /// <summary>
        /// Sadece belirli bir rafta bulunan stok kayıtlarını listeler.
        /// </summary>
        public async Task<IEnumerable<StockDto>> GetAllByShelfIdAsync(Guid shelfId)
        {
            var stocks = await _stockRepository.Where(s => s.ShelfId == shelfId && s.IsActive).ToListAsync();
            return _mapper.Map<IEnumerable<StockDto>>(stocks);
        }

        /// <summary>
        /// Sistemdeki tüm aktif stokları sayfalama altyapısı ile getirir.
        /// </summary>
        public async Task<PagedResult<StockDto>> GetPagedAsync(PaginationParams paginationParams)
        {
            var pagedEntities = await _stockRepository.GetPagedAsync(
                paginationParams,
                filter: s => s.IsActive
            );

            return _mapper.Map<PagedResult<StockDto>>(pagedEntities);
        }

        /// <summary>
        /// Sadece belirli bir ürüne ait stok kayıtlarını sayfalayarak getirir.
        /// (Örn: "iPhone 15 pro nerelerde var?" sayfasındaki tablo için)
        /// </summary>
        public async Task<PagedResult<StockDto>> GetPagedByProductIdAsync(PaginationParams paginationParams, Guid productId)
        {
            var pagedEntities = await _stockRepository.GetPagedAsync(
                paginationParams,
                filter: s => s.IsActive && s.ProductId == productId
            );

            return _mapper.Map<PagedResult<StockDto>>(pagedEntities);
        }

        /// <summary>
        /// Sadece belirli bir deponun içindeki stok kayıtlarını sayfalayarak getirir.
        /// </summary>
        public async Task<PagedResult<StockDto>> GetPagedByWarehouseIdAsync(PaginationParams paginationParams, Guid warehouseId)
        {
            var pagedEntities = await _stockRepository.GetPagedAsync(
                paginationParams,
                filter: s => s.IsActive && s.WarehouseId == warehouseId
            );

            return _mapper.Map<PagedResult<StockDto>>(pagedEntities);
        }

        /// <summary>
        /// Sadece belirli bir raftaki stok kayıtlarını sayfalayarak getirir.
        /// </summary>
        public async Task<PagedResult<StockDto>> GetPagedByShelfIdAsync(PaginationParams paginationParams, Guid shelfId)
        {
            var pagedEntities = await _stockRepository.GetPagedAsync(
                paginationParams,
                filter: s => s.IsActive && s.ShelfId == shelfId
            );

            return _mapper.Map<PagedResult<StockDto>>(pagedEntities);
        }

        /// <summary>
        /// Mevcut bir stok kaydının miktarını, lokasyonunu veya ürününü günceller.
        /// Eski lokasyondan kapasiteleri düşerken, yeni lokasyona kapasite ekler.
        /// </summary>
        public async Task<StockDto> UpdateAsync(StockUpdateDto updateDto)
        {
            ValidateReservedQuantity(updateDto.Quantity, updateDto.ReservedQuantity);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var stock = await _stockRepository.Where(s => s.Id == updateDto.Id && s.IsActive).SingleOrDefaultAsync();
                if (stock == null) throw new ClientSideException("Stok kaydı bulunamadı.");

                // Yeni girilen ilişkili verilerin doğrulanması
                var product = await GetActiveProductAsync(updateDto.ProductId);
                var newWarehouse = await GetActiveWarehouseAsync(updateDto.WarehouseId);
                var newShelf = await GetActiveShelfWithZoneAsync(updateDto.ShelfId, updateDto.WarehouseId);

                // Lokasyon değişimi varsa, yeni rafta bu ürünün önceden eklenmiş bir satırı olup olmadığını kontrol eder.
                await CheckDuplicateStockAsync(updateDto.ProductId, updateDto.ShelfId, updateDto.Id);

                // --- KAPASİTE YÖNETİMİ (SWAP İŞLEMİ) ---
                // Adım 1: Güncelleme öncesi mevcut (eski) durumun kapladığı alanı rafdan ve depodan geri çıkar (Boşalt).
                var oldProduct = await GetActiveProductAsync(stock.ProductId);
                var oldWarehouse = stock.WarehouseId == updateDto.WarehouseId ? newWarehouse : await GetActiveWarehouseAsync(stock.WarehouseId);
                var oldShelf = stock.ShelfId == updateDto.ShelfId ? newShelf : await GetActiveShelfWithZoneAsync(stock.ShelfId, stock.WarehouseId);

                DecreaseCapacity(oldShelf, oldWarehouse, oldProduct, stock.Quantity);

                // Adım 2: Güncellenmiş (yeni) değerlerin kaplayacağı alanı yeni rafa ve depoya ekle (Doldur).
                IncreaseCapacity(newShelf, newWarehouse, product, updateDto.Quantity);

                // Adım 3: Stok entity'sinin property'lerini güncelle.
                stock.ProductId = updateDto.ProductId;
                stock.WarehouseId = updateDto.WarehouseId;
                stock.ShelfId = updateDto.ShelfId;
                stock.Quantity = updateDto.Quantity;
                stock.ReservedQuantity = updateDto.ReservedQuantity;
                stock.LastMovementDate = DateTime.UtcNow;

                _stockRepository.Update(stock);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return _mapper.Map<StockDto>(stock);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Mevcut stok kaydını sistemden siler (Soft Delete).
        /// Silinen ürünün kapladığı alanı deponun ve rafın doluluğundan düşer.
        /// </summary>
        public async Task RemoveAsync(Guid id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var stock = await _stockRepository.Where(s => s.Id == id && s.IsActive).SingleOrDefaultAsync();
                if (stock == null) throw new ClientSideException("Silinmek istenen stok kaydı bulunamadı.");

                // İş Kuralı: İçinde hala ürün bulunan (miktarı > 0) stok kaydı direkt silinemez.
                if (stock.Quantity > 0)
                    throw new ClientSideException("Miktarı 0'dan büyük olan stoklar silinemez. Önce ürün çıkışı (transfer) yapılmalıdır.");

                // Stok miktarı 0 olsa bile tutarlılık adına eski kayıtları çekip kapasiteleri eksi yönde güncelliyoruz.
                var oldProduct = await GetActiveProductAsync(stock.ProductId);
                var oldWarehouse = await GetActiveWarehouseAsync(stock.WarehouseId);
                var oldShelf = await GetActiveShelfWithZoneAsync(stock.ShelfId, stock.WarehouseId);

                DecreaseCapacity(oldShelf, oldWarehouse, oldProduct, stock.Quantity);

                stock.IsActive = false; // Veritabanından tamamen silmek yerine pasife çekiyoruz (Soft delete).
                _stockRepository.Update(stock);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ==========================================
        // PRIVATE YARDIMCI METOTLAR (CLEAN CODE)
        // Bu metotlar kod tekrarını önler ve okunabilirliği artırır.
        // ==========================================

        /// <summary>
        /// Rezerve edilen miktar ile fiziksel miktar arasındaki mantığı doğrular.
        /// </summary>
        private void ValidateReservedQuantity(int quantity, int reservedQuantity)
        {
            if (reservedQuantity > quantity)
                throw new ClientSideException("Rezerve miktar, raftaki fiziksel toplam miktardan büyük olamaz.");
        }

        /// <summary>
        /// Verilen ID'ye göre ürünün sistemde aktif olarak var olup olmadığını doğrular.
        /// </summary>
        private async Task<Product> GetActiveProductAsync(Guid productId)
        {
            var product = await _context.Set<Product>().SingleOrDefaultAsync(p => p.Id == productId && p.IsActive);
            if (product == null) throw new ClientSideException("Ürün sistemde bulunamadı veya pasif durumda.");
            return product;
        }

        /// <summary>
        /// Verilen ID'ye göre deponun sistemde aktif olarak var olup olmadığını doğrular.
        /// </summary>
        private async Task<Warehouse> GetActiveWarehouseAsync(Guid warehouseId)
        {
            var warehouse = await _context.Set<Warehouse>().FirstOrDefaultAsync(w => w.Id == warehouseId && w.IsActive);
            if (warehouse == null) throw new ClientSideException("Depo sistemde bulunamadı veya pasif durumda.");
            return warehouse;
        }

        /// <summary>
        /// Verilen Rafın sistemde aktif olarak bulunduğunu ve iddia edilen Depo'ya ait olduğunu doğrular.
        /// </summary>
        private async Task<Shelf> GetActiveShelfWithZoneAsync(Guid shelfId, Guid expectedWarehouseId)
        {
            // Sadece rafı değil, bağlı olduğu alanı (WarehouseZone) da çekiyoruz ki Depo ID'sine ulaşabilelim.
            var shelf = await _context.Set<Shelf>()
                .Include(s => s.WarehouseZone)
                .FirstOrDefaultAsync(s => s.Id == shelfId && s.IsActive);

            if (shelf == null) throw new ClientSideException("Raf sistemde bulunamadı veya pasif durumda.");

            // İş Kuralı: Seçilen rafın bağlı olduğu Zone'un Depo ID'si, kullanıcının gönderdiği Depo ID ile eşleşmeli.
            if (shelf.WarehouseZone.WarehouseId != expectedWarehouseId)
                throw new ClientSideException("Seçilen raf, belirtilen depoya ait değildir!");

            return shelf;
        }

        /// <summary>
        /// Aynı rafta, aynı ürüne ait ikinci bir satırın oluşturulmasını engeller.
        /// Update işlemi sırasında mevcut kaydın (currentStockId) kendisi yoksayılır.
        /// </summary>
        private async Task CheckDuplicateStockAsync(Guid productId, Guid shelfId, Guid? currentStockId = null)
        {
            var query = _stockRepository.Where(s => s.ProductId == productId && s.ShelfId == shelfId && s.IsActive);

            if (currentStockId.HasValue)
                query = query.Where(s => s.Id != currentStockId.Value);

            bool isDuplicate = await query.AnyAsync();
            if (isDuplicate)
                throw new ClientSideException("Hedef rafta bu üründen zaten bir kayıt var. İki ayrı satır oluşturulamaz.");
        }

        /// <summary>
        /// Belirtilen raf ve deponun doluluk oranlarını (Hacim ve Ağırlık) artırır.
        /// Maksimum kapasite aşılırsa hata fırlatır.
        /// </summary>
        private void IncreaseCapacity(Shelf shelf, Warehouse warehouse, Product product, int quantity)
        {
            double totalWeight = product.Weight * quantity;
            double totalVolume = product.Volume * quantity; // Product entity'sindeki hesaplanmış (Width*Height*Depth) Volume özelliği kullanılır.

            if (shelf.CurrentWeight + totalWeight > shelf.MaxWeight)
                throw new ClientSideException($"Rafın maksimum ağırlık kapasitesi aşılıyor. Kalan: {shelf.MaxWeight - shelf.CurrentWeight} birim.");

            if (shelf.CurrentVolume + totalVolume > shelf.MaxVolume)
                throw new ClientSideException($"Rafın maksimum hacim kapasitesi aşılıyor. Kalan: {shelf.MaxVolume - shelf.CurrentVolume} birim.");

            if (warehouse.UsedCapacity + totalVolume > warehouse.MaxCapacity)
                throw new ClientSideException("Deponun genel kapasitesi aşılıyor!");

            shelf.CurrentWeight += totalWeight;
            shelf.CurrentVolume += totalVolume;
            warehouse.UsedCapacity += totalVolume;
        }

        /// <summary>
        /// Belirtilen raf ve deponun doluluk oranlarını (Hacim ve Ağırlık) azaltır.
        /// </summary>
        private void DecreaseCapacity(Shelf shelf, Warehouse warehouse, Product product, int quantity)
        {
            double totalWeight = product.Weight * quantity;
            double totalVolume = product.Volume * quantity;

            shelf.CurrentWeight -= totalWeight;
            if (shelf.CurrentWeight < 0) shelf.CurrentWeight = 0; // Matematiksel olarak negatife düşme ihtimaline karşı güvenlik kalkanı.

            shelf.CurrentVolume -= totalVolume;
            if (shelf.CurrentVolume < 0) shelf.CurrentVolume = 0;

            warehouse.UsedCapacity -= totalVolume;
            if (warehouse.UsedCapacity < 0) warehouse.UsedCapacity = 0;
        }
    }
}