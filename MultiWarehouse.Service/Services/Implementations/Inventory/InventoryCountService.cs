using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MultiWarehouse.Entity.Entities.Definitions;
using MultiWarehouse.Entity.Entities.Identity;
using MultiWarehouse.Entity.Entities.Inventory;
using MultiWarehouse.Entity.Enums.Common;
using MultiWarehouse.Entity.Enums.Inventory;
using MultiWarehouse.Entity.Enums.User;
using MultiWarehouse.Service.Exceptions;
using MultiWarehouse.Service.Repositories.Interfaces;
using MultiWarehouse.Service.Services.Interfaces.Inventory;
using MultiWarehouse.Service.Services.Interfaces.Notification;
using MultiWarehouse.Shared.DTOs.CountDtos;
using MultiWarehouse.Shared.DTOs.NotificationDtos;
using System.Security.Claims;

namespace MultiWarehouse.Service.Services.Implementations.Inventory
{
    public class InventoryCountService : IInventoryCountService
    {
        #region Dependencies

        private readonly IGenericRepository<Product> _productRepository;
        private readonly IGenericRepository<Shelf> _shelfRepository;
        private readonly IGenericRepository<Warehouse> _warehouseRepository;
        private readonly IGenericRepository<Stock> _stockRepository;
        private readonly IGenericRepository<InventoryCount> _inventoryCountRepository;
        private readonly IGenericRepository<StockMovement> _stockMovementRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public InventoryCountService(
            IGenericRepository<Product> productRepository,
            IGenericRepository<Shelf> shelfRepository,
            IGenericRepository<Warehouse> warehouseRepository,
            IGenericRepository<Stock> stockRepository,
            IGenericRepository<InventoryCount> inventoryCountRepository,
            IGenericRepository<StockMovement> stockMovementRepository,
            IGenericRepository<User> userRepository,
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            IHttpContextAccessor httpContextAccessor)
        {
            _productRepository = productRepository;
            _shelfRepository = shelfRepository;
            _warehouseRepository = warehouseRepository;
            _stockRepository = stockRepository;
            _inventoryCountRepository = inventoryCountRepository;
            _stockMovementRepository = stockMovementRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _httpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region Command Operations

        public async Task<InventoryCountResultDto> PerformCountAsync(InventoryCountCreateDto countDto)
        {
            var userId = GetCurrentUserId();
            ValidateRowLevelSecurity(countDto.WarehouseId);

            var product = await _productRepository.Where(p => p.Id == countDto.ProductId).SingleOrDefaultAsync();
            var shelf = await _shelfRepository.Where(s => s.Id == countDto.ShelfId).Include(s => s.WarehouseZone).SingleOrDefaultAsync();
            var warehouse = await _warehouseRepository.Where(w => w.Id == countDto.WarehouseId).SingleOrDefaultAsync();

            if (product == null) throw new ClientSideException("Ürün bulunamadı.");
            if (shelf == null || !shelf.IsActive) throw new ClientSideException("Raf bulunamadı veya pasif durumda.");
            if (warehouse == null || !warehouse.IsActive) throw new ClientSideException("Depo bulunamadı veya pasif durumda.");
            if (shelf.WarehouseZone?.WarehouseId != countDto.WarehouseId) throw new ClientSideException("Seçilen raf, seçilen depoya ait değil!");

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var stock = await _stockRepository
                    .Where(s => s.WarehouseId == countDto.WarehouseId &&
                                s.ShelfId == countDto.ShelfId &&
                                s.ProductId == countDto.ProductId &&
                                s.IsActive)
                    .SingleOrDefaultAsync();

                int systemQuantity = stock?.Quantity ?? 0;
                int variance = countDto.CountedQuantity - systemQuantity;

                if (stock != null && countDto.CountedQuantity < stock.ReservedQuantity)
                {
                    throw new ClientSideException($"Kritik Hata: Sayılan miktar ({countDto.CountedQuantity}), rezerve stoktan ({stock.ReservedQuantity}) daha az olamaz! Bekleyen fişleri iptal edin.");
                }

                CountStatus status = CountStatus.Matched;
                string movementDescription = "Inventory Count - Matched";

                if (variance < 0)
                {
                    status = CountStatus.Shortage;
                    movementDescription = $"Inventory Count Adjustment - Shortage (Sistem: {systemQuantity}, Sayılan: {countDto.CountedQuantity})";
                }
                else if (variance > 0)
                {
                    status = CountStatus.Overage;
                    movementDescription = $"Inventory Count Adjustment - Overage (Sistem: {systemQuantity}, Sayılan: {countDto.CountedQuantity})";
                }

                var countRecord = new InventoryCount
                {
                    WarehouseId = countDto.WarehouseId,
                    ShelfId = countDto.ShelfId,
                    ProductId = countDto.ProductId,
                    SystemQuantity = systemQuantity,
                    CountedQuantity = countDto.CountedQuantity,
                    Variance = variance,
                    Status = status,
                    UserId = userId,
                    CreatedDate = DateTime.UtcNow,
                    Description = movementDescription
                };
                await _inventoryCountRepository.AddAsync(countRecord);

                if (variance != 0)
                {
                    if (stock == null)
                    {
                        stock = new Stock
                        {
                            WarehouseId = countDto.WarehouseId,
                            ShelfId = countDto.ShelfId,
                            ProductId = countDto.ProductId,
                            Quantity = countDto.CountedQuantity,
                            ReservedQuantity = 0,
                            CreatedDate = DateTime.UtcNow,
                            IsActive = true
                        };
                        await _stockRepository.AddAsync(stock);
                    }
                    else
                    {
                        stock.Quantity = countDto.CountedQuantity;
                        stock.UpdatedDate = DateTime.UtcNow;
                        _stockRepository.Update(stock);
                    }

                    double unitVolume = product.Width * product.Height * product.Depth;
                    double volumeDiff = unitVolume * variance;
                    double weightDiff = product.Weight * variance;

                    shelf.CurrentVolume += volumeDiff;
                    shelf.CurrentWeight += weightDiff;
                    if (shelf.CurrentVolume < 0) shelf.CurrentVolume = 0;
                    if (shelf.CurrentWeight < 0) shelf.CurrentWeight = 0;
                    shelf.UpdatedDate = DateTime.UtcNow;
                    _shelfRepository.Update(shelf);

                    warehouse.UsedCapacity += volumeDiff;
                    if (warehouse.UsedCapacity < 0) warehouse.UsedCapacity = 0;
                    warehouse.UpdatedDate = DateTime.UtcNow;
                    _warehouseRepository.Update(warehouse);

                    var movementType = variance > 0 ? MovementType.AdjustmentIn : MovementType.AdjustmentOut;

                    var stockMovement = new StockMovement
                    {
                        WarehouseId = countDto.WarehouseId,
                        ProductId = countDto.ProductId,
                        ShelfId = countDto.ShelfId,
                        MovementType = movementType,
                        Quantity = Math.Abs(variance),
                        Description = movementDescription,
                        UserId = userId,
                        CreatedDate = DateTime.UtcNow,
                        IsActive = true
                    };
                    await _stockMovementRepository.AddAsync(stockMovement);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                if (variance != 0)
                {
                    string alertType = variance < 0 ? "EKSİK" : "FAZLA";

                    await NotifyWarehouseManagersAsync(
                        countDto.WarehouseId,
                        $"Sayım Uyarısı: Stok {alertType} Çıktı!",
                        $"{product.Name} (SKU: {product.Sku}) ürünü için {shelf.ShelfNumber} rafında {Math.Abs(variance)} adet {alertType.ToLower()} tespit edildi.",
                        NotificationType.Security,
                        NotificationTargetType.Warehouse,
                        countDto.WarehouseId
                    );
                }

                return new InventoryCountResultDto
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Sku = product.Sku,
                    ShelfCode = shelf.ShelfNumber,
                    SystemQuantity = systemQuantity,
                    CountedQuantity = countDto.CountedQuantity,
                    Variance = variance,
                    Status = status

                };
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        #endregion

        #region Private Helpers

        private Guid GetCurrentUserId()
        {
            var userIdString = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdString, out var userId))
                return userId;

            throw new UnauthorizedAccessException("Kullanıcı kimliği doğrulanamadı.");
        }

        private void ValidateRowLevelSecurity(Guid requestedWarehouseId)
        {
            var currentUserRole = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;
            if (currentUserRole != UserRole.SuperAdmin.ToString())
            {
                var claim = _httpContextAccessor.HttpContext?.User.FindFirst("WarehouseId");

                if (claim == null || !Guid.TryParse(claim.Value, out var currentWarehouseId))
                    throw new UnauthorizedAccessException("Kullanıcıya ait depo bilgisi bulunamadı. İşlem reddedildi.");

                if (currentWarehouseId != requestedWarehouseId)
                    throw new ClientSideException("Başka bir deponun sayımını gerçekleştirme yetkiniz bulunmamaktadır.");
            }
        }

        private async Task NotifyWarehouseManagersAsync(Guid warehouseId, string title, string message, NotificationType type, NotificationTargetType targetType, Guid? targetId)
        {
            var targetUsers = await _userRepository
                .Where(u => u.IsActive &&
                           (u.Role == UserRole.SuperAdmin ||
                           (u.Role == UserRole.WarehouseManager && u.WarehouseId == warehouseId)))
                .ToListAsync();

            foreach (var user in targetUsers)
            {
                await _notificationService.CreateAsync(new NotificationCreateDto
                {
                    UserId = user.Id,
                    Title = title,
                    Message = message,
                    Type = type,
                    TargetType = targetType,
                    TargetId = targetId
                });
            }
        }

        #endregion
    }
}