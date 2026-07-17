// MultiWarehouse.Service/Services/Implementations/StockMovementService.cs
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MultiWarehouse.Entity.Entities;
using MultiWarehouse.Entity.Enums;
using MultiWarehouse.Service.Context;
using MultiWarehouse.Service.Exceptions;
using MultiWarehouse.Service.Repositories.Interfaces;
using MultiWarehouse.Service.Services.Interfaces;
using MultiWarehouse.Shared.DTOs.StockMovementDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MultiWarehouse.Service.Services.Implementations
{
    public class StockMovementService : IStockMovementService
    {
        private readonly IGenericRepository<StockMovement> _movementRepository;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public StockMovementService(IGenericRepository<StockMovement> movementRepository, AppDbContext context, IMapper mapper)
        {
            _movementRepository = movementRepository;
            _context = context;
            _mapper = mapper;
        }

        public async Task<StockMovementDto> CreateAsync(StockMovementCreateDto createDto)
        {
            if (createDto.Quantity <= 0)
                throw new ClientSideException("Hareket miktarı 0'dan büyük olmalıdır.");

            // TRANSACTION BAŞLANGICI: Hata olursa ne raf kapasitesi değişir ne stok düşer ne de hareket kaydı atılır!
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userExists = await _context.Set<User>().AnyAsync(u => u.Id == createDto.UserId && u.IsActive);
                if (!userExists) throw new ClientSideException("İşlemi yapan kullanıcı sistemde bulunamadı.");

                var product = await _context.Set<Product>().FirstOrDefaultAsync(p => p.Id == createDto.ProductId && p.IsActive);
                if (product == null) throw new ClientSideException("İşlem yapılmak istenen ürün sistemde bulunamadı.");

                // HAREKET TÜRÜNE GÖRE İLGİLİ İŞ METODUNU (HANDLER) ÇAĞIR
                switch (createDto.MovementType)
                {
                    case MovementType.Inbound:
                    case MovementType.CustomerReturn:
                        await ProcessInboundAsync(createDto, product);
                        break;

                    case MovementType.Outbound:
                    case MovementType.SupplierReturn:
                    case MovementType.Scrap:
                        await ProcessOutboundAsync(createDto, product);
                        break;

                    case MovementType.WarehouseTransfer:
                    case MovementType.ShelfTransfer:
                        await ProcessTransferAsync(createDto, product);
                        break;

                    case MovementType.Adjustment:
                        // Adjustment (Sayım Düzeltmesi) karmaşıktır. Source varsa düşer, Destination varsa ekler.
                        if (createDto.SourceShelfId.HasValue)
                            await ProcessOutboundAsync(createDto, product); // Sayımda eksik çıktı (Stoktan düş)
                        else if (createDto.DestinationShelfId.HasValue)
                            await ProcessInboundAsync(createDto, product);  // Sayımda fazla çıktı (Stoka ekle)
                        else
                            throw new ClientSideException("Sayım düzeltmesi için Kaynak veya Hedef lokasyon belirtilmelidir.");
                        break;
                }

                // Tüm işlemler başarılıysa hareketi (Defteri) kaydet
                var movement = _mapper.Map<StockMovement>(createDto);
                await _movementRepository.AddAsync(movement);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return _mapper.Map<StockMovementDto>(movement);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ==========================================
        // PRIVATE HANDLER METOTLARI (Gerçek WMS Mantığı)
        // ==========================================

        /// <summary>
        /// İçeriye mal girişini (Kapasite artırma ve Stok yaratma/artırma) yönetir.
        /// </summary>
        private async Task ProcessInboundAsync(StockMovementCreateDto dto, Product product)
        {
            if (!dto.DestinationWarehouseId.HasValue || !dto.DestinationShelfId.HasValue)
                throw new ClientSideException($"{dto.MovementType} işlemlerinde Hedef Depo ve Hedef Raf zorunludur.");

            var warehouse = await GetWarehouseAsync(dto.DestinationWarehouseId.Value);
            var shelf = await GetShelfAsync(dto.DestinationShelfId.Value, warehouse.Id);

            // 1. Kapasite Kontrolü ve Güncellemesi
            IncreaseCapacity(shelf, warehouse, product, dto.Quantity);

            // 2. Stok Kontrolü: Bu rafta bu üründen zaten var mı?
            var existingStock = await _context.Set<Stock>().FirstOrDefaultAsync(s => s.ProductId == product.Id && s.ShelfId == shelf.Id && s.IsActive);

            if (existingStock != null)
            {
                // Varsa miktarını artır
                existingStock.Quantity += dto.Quantity;
                existingStock.LastMovementDate = DateTime.UtcNow;
                _context.Set<Stock>().Update(existingStock);
            }
            else
            {
                // Yoksa sıfırdan stok satırı oluştur
                var newStock = new Stock
                {
                    ProductId = product.Id,
                    WarehouseId = warehouse.Id,
                    ShelfId = shelf.Id,
                    Quantity = dto.Quantity,
                    ReservedQuantity = 0,
                    LastMovementDate = DateTime.UtcNow
                };
                await _context.Set<Stock>().AddAsync(newStock);
            }
        }

        /// <summary>
        /// Dışarıya mal çıkışını (Kapasite düşürme, Stok kullanılabilir miktar kontrolü) yönetir.
        /// </summary>
        private async Task ProcessOutboundAsync(StockMovementCreateDto dto, Product product)
        {
            if (!dto.SourceWarehouseId.HasValue || !dto.SourceShelfId.HasValue)
                throw new ClientSideException($"{dto.MovementType} işlemlerinde Kaynak Depo ve Kaynak Raf zorunludur.");

            var warehouse = await GetWarehouseAsync(dto.SourceWarehouseId.Value);
            var shelf = await GetShelfAsync(dto.SourceShelfId.Value, warehouse.Id);

            // 1. İlgili stok kaydı rafta gerçekten var mı?
            var existingStock = await _context.Set<Stock>().FirstOrDefaultAsync(s => s.ProductId == product.Id && s.ShelfId == shelf.Id && s.IsActive);
            if (existingStock == null)
                throw new ClientSideException("Belirtilen rafta bu ürüne ait stok bulunmamaktadır.");

            // 2. Çıkarılmak istenen miktar, rezerve edilmiş miktar düşüldükten sonraki 'kullanılabilir' stoktan büyük olamaz!
            int availableQuantity = existingStock.Quantity - existingStock.ReservedQuantity;
            if (dto.Quantity > availableQuantity)
                throw new ClientSideException($"Yetersiz stok! Rafta {existingStock.Quantity} adet var, ancak {existingStock.ReservedQuantity} adedi rezerve. Çıkarılabilecek maksimum miktar: {availableQuantity}");

            // 3. Kapasiteleri Düşür ve Stoku Güncelle
            DecreaseCapacity(shelf, warehouse, product, dto.Quantity);

            existingStock.Quantity -= dto.Quantity;
            existingStock.LastMovementDate = DateTime.UtcNow;
            _context.Set<Stock>().Update(existingStock);
        }

        /// <summary>
        /// Aynı anda hem çıkış hem giriş (Transfer) işlemlerini yönetir.
        /// </summary>
        private async Task ProcessTransferAsync(StockMovementCreateDto dto, Product product)
        {
            if (!dto.SourceShelfId.HasValue || !dto.DestinationShelfId.HasValue)
                throw new ClientSideException("Transfer işlemlerinde Kaynak ve Hedef raflar zorunludur.");

            if (dto.SourceShelfId == dto.DestinationShelfId)
                throw new ClientSideException("Kaynak raf ile Hedef raf aynı olamaz.");

            // Transfer = Kaynaktan Çıkış + Hedefe Giriş
            await ProcessOutboundAsync(dto, product);
            await ProcessInboundAsync(dto, product);
        }

        // ==========================================
        // YARDIMCI METOTLAR (VALIDATION & CAPACITY)
        // ==========================================

        private async Task<Warehouse> GetWarehouseAsync(Guid warehouseId)
        {
            var warehouse = await _context.Set<Warehouse>().FirstOrDefaultAsync(w => w.Id == warehouseId && w.IsActive);
            if (warehouse == null) throw new ClientSideException("Belirtilen depo sistemde bulunamadı.");
            return warehouse;
        }

        private async Task<Shelf> GetShelfAsync(Guid shelfId, Guid expectedWarehouseId)
        {
            var shelf = await _context.Set<Shelf>().Include(s => s.WarehouseZone).FirstOrDefaultAsync(s => s.Id == shelfId && s.IsActive);
            if (shelf == null) throw new ClientSideException("Belirtilen raf sistemde bulunamadı.");
            if (shelf.WarehouseZone.WarehouseId != expectedWarehouseId)
                throw new ClientSideException("Seçilen raf, belirtilen depoya ait değildir!");

            return shelf;
        }

        private void IncreaseCapacity(Shelf shelf, Warehouse warehouse, Product product, int quantity)
        {
            double totalWeight = product.Weight * quantity;
            double totalVolume = product.Volume * quantity;

            if (shelf.CurrentWeight + totalWeight > shelf.MaxWeight)
                throw new ClientSideException($"Hedef rafın maksimum ağırlık kapasitesi aşılıyor. Kalan: {shelf.MaxWeight - shelf.CurrentWeight}");

            if (shelf.CurrentVolume + totalVolume > shelf.MaxVolume)
                throw new ClientSideException($"Hedef rafın maksimum hacim kapasitesi aşılıyor. Kalan: {shelf.MaxVolume - shelf.CurrentVolume}");

            if (warehouse.UsedCapacity + totalVolume > warehouse.MaxCapacity)
                throw new ClientSideException("Hedef deponun genel kapasitesi aşılıyor!");

            shelf.CurrentWeight += totalWeight;
            shelf.CurrentVolume += totalVolume;
            warehouse.UsedCapacity += totalVolume;

            _context.Set<Shelf>().Update(shelf);
            _context.Set<Warehouse>().Update(warehouse);
        }

        private void DecreaseCapacity(Shelf shelf, Warehouse warehouse, Product product, int quantity)
        {
            double totalWeight = product.Weight * quantity;
            double totalVolume = product.Volume * quantity;

            shelf.CurrentWeight -= totalWeight;
            if (shelf.CurrentWeight < 0) shelf.CurrentWeight = 0;

            shelf.CurrentVolume -= totalVolume;
            if (shelf.CurrentVolume < 0) shelf.CurrentVolume = 0;

            warehouse.UsedCapacity -= totalVolume;
            if (warehouse.UsedCapacity < 0) warehouse.UsedCapacity = 0;

            _context.Set<Shelf>().Update(shelf);
            _context.Set<Warehouse>().Update(warehouse);
        }

        // ==========================================
        // DİĞER STANDART CRUD İŞLEMLERİ
        // ==========================================

        public async Task<StockMovementDto> GetByIdAsync(Guid id)
        {
            var movement = await _movementRepository.Where(m => m.Id == id && m.IsActive).SingleOrDefaultAsync();
            if (movement == null) throw new ClientSideException("Stok hareketi bulunamadı.");
            return _mapper.Map<StockMovementDto>(movement);
        }

        public async Task<IEnumerable<StockMovementDto>> GetAllAsync()
        {
            var movements = await _movementRepository.Where(m => m.IsActive).ToListAsync();
            return _mapper.Map<IEnumerable<StockMovementDto>>(movements);
        }

        public async Task<IEnumerable<StockMovementDto>> GetAllByProductIdAsync(Guid productId)
        {
            var movements = await _movementRepository.Where(m => m.ProductId == productId && m.IsActive).ToListAsync();
            return _mapper.Map<IEnumerable<StockMovementDto>>(movements);
        }

        public async Task<IEnumerable<StockMovementDto>> GetAllByWarehouseIdAsync(Guid warehouseId)
        {
            var movements = await _movementRepository.Where(m =>
                (m.SourceWarehouseId == warehouseId || m.DestinationWarehouseId == warehouseId) && m.IsActive).ToListAsync();
            return _mapper.Map<IEnumerable<StockMovementDto>>(movements);
        }

        public async Task<StockMovementDto> UpdateAsync(StockMovementUpdateDto updateDto)
        {
            var movement = await _movementRepository.Where(m => m.Id == updateDto.Id && m.IsActive).SingleOrDefaultAsync();
            if (movement == null) throw new ClientSideException("Güncellenmek istenen stok hareketi bulunamadı.");

            // Geçmiş bir hareketin miktarını veya ürününü değiştirmek WMS sistemlerinde yasaktır. 
            // Yanlışlık varsa "Adjustment" veya "Transfer" ile yeni hareket oluşturularak düzeltilir.
            movement.ReferenceNo = updateDto.ReferenceNo;
            movement.Status = updateDto.Status;
            movement.Description = updateDto.Description;

            _movementRepository.Update(movement);
            await _context.SaveChangesAsync();

            return _mapper.Map<StockMovementDto>(movement);
        }

        public async Task RemoveAsync(Guid id)
        {
            var movement = await _movementRepository.Where(m => m.Id == id && m.IsActive).SingleOrDefaultAsync();
            if (movement == null) throw new ClientSideException("Silinmek istenen stok hareketi bulunamadı.");

            // Not: İdeal sistemlerde hareket kaydı hiç silinmez (iptal edilir), ancak CRUD gereği Soft Delete uygulanmıştır.
            movement.IsActive = false;
            _movementRepository.Update(movement);
            await _context.SaveChangesAsync();
        }
    }
}