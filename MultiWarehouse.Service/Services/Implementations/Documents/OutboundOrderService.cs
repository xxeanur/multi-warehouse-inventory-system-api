using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MultiWarehouse.Entity.Entities.Definitions;
using MultiWarehouse.Entity.Entities.Documents;
using MultiWarehouse.Entity.Entities.Identity;
using MultiWarehouse.Entity.Entities.Inventory;
using MultiWarehouse.Entity.Enums.Common;
using MultiWarehouse.Entity.Enums.Document;
using MultiWarehouse.Entity.Enums.User;
using MultiWarehouse.Service.Exceptions;
using MultiWarehouse.Service.Repositories.Interfaces;
using MultiWarehouse.Service.Services.Interfaces.Documents;
using MultiWarehouse.Service.Services.Interfaces.Notification;
using MultiWarehouse.Shared.DTOs.DocumentDtos.OutboundDtos;
using MultiWarehouse.Shared.DTOs.NotificationDtos;
using MultiWarehouse.Shared.Pagination;
using System.Security.Claims;

namespace MultiWarehouse.Service.Services.Implementations.Documents
{
    public class OutboundOrderService : IOutboundOrderService
    {
        #region Dependencies

        private readonly IGenericRepository<OutboundOrder> _outboundRepository;
        private readonly IGenericRepository<OutboundOrderReservation> _reservationRepository;
        private readonly IGenericRepository<Stock> _stockRepository;
        private readonly IGenericRepository<Warehouse> _warehouseRepository;
        private readonly IGenericRepository<Shelf> _shelfRepository;
        private readonly IGenericRepository<Product> _productRepository;
        private readonly IGenericRepository<StockMovement> _stockMovementRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OutboundOrderService(
            IGenericRepository<OutboundOrder> outboundRepository,
            IGenericRepository<OutboundOrderReservation> reservationRepository,
            IGenericRepository<Stock> stockRepository,
            IGenericRepository<Warehouse> warehouseRepository,
            IGenericRepository<Shelf> shelfRepository,
            IGenericRepository<Product> productRepository,
            IGenericRepository<StockMovement> stockMovementRepository,
            IGenericRepository<User> userRepository,
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            IHttpContextAccessor httpContextAccessor)
        {
            _outboundRepository = outboundRepository;
            _reservationRepository = reservationRepository;
            _stockRepository = stockRepository;
            _warehouseRepository = warehouseRepository;
            _shelfRepository = shelfRepository;
            _productRepository = productRepository;
            _stockMovementRepository = stockMovementRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _httpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region Write Operations

        public async Task<Guid> CreateAsync(OutboundOrderCreateDto createDto)
        {
            ValidateRowLevelSecurity(createDto.WarehouseId);

            var duplicateCheck = createDto.Lines.GroupBy(l => l.ProductId).Any(g => g.Count() > 1);
            if (duplicateCheck)
                throw new ClientSideException("Bir ürün bir siparişte yalnızca tek bir satırda gönderilebilir. Lütfen miktarları toplayıp tek satır olarak gönderin.");

            var productIds = createDto.Lines.Select(l => l.ProductId).ToList();

            var allAvailableStocks = await _stockRepository
                .Where(s => s.WarehouseId == createDto.WarehouseId && productIds.Contains(s.ProductId) && s.IsActive)
                .ToListAsync();

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var orderId = Guid.NewGuid();
                var order = new OutboundOrder
                {
                    Id = orderId,
                    DocumentNumber = await GenerateDocumentNumberAsync(),
                    Destination = createDto.Destination,
                    WarehouseId = createDto.WarehouseId,
                    MovementType = createDto.MovementType,
                    Description = createDto.Description,
                    Status = DocumentStatus.Pending,
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true,
                    CreatedById = GetCurrentUserId()
                };

                foreach (var lineDto in createDto.Lines)
                {
                    order.Lines.Add(new OutboundOrderLine
                    {
                        ProductId = lineDto.ProductId,
                        RequestedQuantity = lineDto.RequestedQuantity,
                        PickedQuantity = 0,
                        CreatedDate = DateTime.UtcNow,
                        IsActive = true
                    });

                    var productStocks = allAvailableStocks.Where(s => s.ProductId == lineDto.ProductId).ToList();
                    var totalAvailable = productStocks.Sum(s => s.Quantity - s.ReservedQuantity);

                    if (totalAvailable < lineDto.RequestedQuantity)
                        throw new ClientSideException($"Yetersiz Stok! Seçilen depoda bu üründen kullanılabilir durumda sadece {totalAvailable} adet bulunuyor.");

                    int remainingToReserve = lineDto.RequestedQuantity;

                    foreach (var stock in productStocks.OrderByDescending(s => s.Quantity - s.ReservedQuantity))
                    {
                        if (remainingToReserve <= 0) break;
                        var availableInShelf = stock.Quantity - stock.ReservedQuantity;

                        if (availableInShelf > 0)
                        {
                            var reserveAmount = Math.Min(availableInShelf, remainingToReserve);

                            stock.ReservedQuantity += reserveAmount;
                            stock.UpdatedDate = DateTime.UtcNow;
                            stock.Version = Guid.NewGuid();
                            _stockRepository.Update(stock);

                            await _reservationRepository.AddAsync(new OutboundOrderReservation
                            {
                                OutboundOrderId = orderId,
                                ProductId = lineDto.ProductId,
                                ShelfId = stock.ShelfId,
                                ReservedQuantity = reserveAmount,
                                CreatedDate = DateTime.UtcNow,
                                IsActive = true
                            });

                            remainingToReserve -= reserveAmount;
                        }
                    }
                }

                await _outboundRepository.AddAsync(order);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                await NotifyWarehouseManagersAsync(order.WarehouseId, "Yeni Mal Çıkış Talebi", $"{order.DocumentNumber} numaralı yeni bir mal çıkış talebi oluşturuldu ve stoklar başarıyla rezerve edildi.", NotificationType.Outbound, NotificationTargetType.OutboundOrder, order.Id);

                return order.Id;
            }
            catch (DbUpdateConcurrencyException)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new ClientSideException("Sistem Meşgul: Seçtiğiniz stoklara şu an başka bir operatör işlem yapıyor. Lütfen tekrar deneyin.");
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task ApproveAsync(OutboundOrderApproveDto approveDto)
        {
            var userId = GetCurrentUserId();

            var order = await _outboundRepository
                .Where(o => o.Id == approveDto.OutboundOrderId)
                .Include(o => o.Lines)
                .SingleOrDefaultAsync();

            if (order == null) throw new ClientSideException("Belge bulunamadı.");
            ValidateRowLevelSecurity(order.WarehouseId);

            if (order.Status != DocumentStatus.Pending)
                throw new ClientSideException("Sadece Beklemede (Pending) olan belgeler onaylanabilir.");

            var warehouse = await _warehouseRepository.Where(w => w.Id == order.WarehouseId).SingleOrDefaultAsync();
            if (warehouse == null) throw new ClientSideException("Çıkış yapılacak depo bulunamadı.");

            var productIds = order.Lines.Select(l => l.ProductId).Distinct().ToList();
            var shelfIds = approveDto.PickedLines.Select(p => p.ShelfId).Distinct().ToList();

            var productsDict = await _productRepository.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);
            var shelvesDict = await _shelfRepository.Where(s => shelfIds.Contains(s.Id)).Include(s => s.WarehouseZone).ToDictionaryAsync(s => s.Id);
            var stocksList = await _stockRepository.Where(s => s.WarehouseId == order.WarehouseId && productIds.Contains(s.ProductId) && s.IsActive).ToListAsync();

            var reservations = await _reservationRepository.Where(r => r.OutboundOrderId == order.Id && r.IsActive).ToListAsync();

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var pickedLinesLookup = approveDto.PickedLines.ToLookup(a => a.OutboundOrderLineId);

                foreach (var line in order.Lines)
                {
                    var lineInputs = pickedLinesLookup[line.Id].ToList();
                    var lineReservations = reservations.Where(r => r.ProductId == line.ProductId).ToList();

                    var totalPickedQuantity = lineInputs.Sum(a => a.Quantity);

                    if (totalPickedQuantity != line.RequestedQuantity)
                        throw new ClientSideException($"Kısmi çıkış kapalıdır! Ürün için istenen: {line.RequestedQuantity}, Toplanan: {totalPickedQuantity}");

                    foreach (var input in lineInputs)
                    {
                        var correspondingReservation = lineReservations.FirstOrDefault(r => r.ShelfId == input.ShelfId);
                        if (correspondingReservation == null || correspondingReservation.ReservedQuantity < input.Quantity)
                        {
                            var expectedShelves = string.Join(", ", lineReservations.Select(r => $"{shelvesDict.GetValueOrDefault(r.ShelfId)?.ShelfNumber} ({r.ReservedQuantity} adet)"));
                            throw new ClientSideException($"Sıkı Tahsis İhlali! Sistem bu ürün için rezervasyonu şu raflardan yaptı: {expectedShelves}. Lütfen belirtilen raflardan toplayınız.");
                        }
                    }
                }

                double totalVolumeToRemove = 0;

                foreach (var line in order.Lines)
                {
                    var lineInputs = pickedLinesLookup[line.Id].ToList();
                    line.PickedQuantity = lineInputs.Sum(a => a.Quantity);

                    if (productsDict.TryGetValue(line.ProductId, out var product))
                    {
                        double unitVolume = product.Width * product.Height * product.Depth;
                        totalVolumeToRemove += (unitVolume * line.PickedQuantity);
                    }

                    foreach (var input in lineInputs)
                    {
                        var stock = stocksList.FirstOrDefault(s => s.ShelfId == input.ShelfId && s.ProductId == line.ProductId);
                        if (stock == null) throw new ClientSideException("Stok bulunamadı.");

                        var reservation = reservations.FirstOrDefault(r => r.ShelfId == input.ShelfId && r.ProductId == line.ProductId);

                        stock.Quantity -= input.Quantity;
                        stock.ReservedQuantity -= input.Quantity;
                        stock.UpdatedDate = DateTime.UtcNow;
                        stock.Version = Guid.NewGuid();
                        _stockRepository.Update(stock);

                        if (reservation != null) _reservationRepository.Remove(reservation);

                        if (shelvesDict.TryGetValue(input.ShelfId, out var shelf))
                        {
                            if (product != null)
                            {
                                double unitVolume = product.Width * product.Height * product.Depth;
                                shelf.CurrentVolume -= (unitVolume * input.Quantity);
                                shelf.CurrentWeight -= (product.Weight * input.Quantity);

                                if (shelf.CurrentVolume < 0) shelf.CurrentVolume = 0;
                                if (shelf.CurrentWeight < 0) shelf.CurrentWeight = 0;

                                shelf.UpdatedDate = DateTime.UtcNow;
                                shelf.Version = Guid.NewGuid();
                                _shelfRepository.Update(shelf);
                            }
                        }

                        await _stockMovementRepository.AddAsync(new StockMovement
                        {
                            WarehouseId = order.WarehouseId,
                            ProductId = line.ProductId,
                            ShelfId = input.ShelfId,
                            MovementType = order.MovementType,
                            Quantity = input.Quantity,
                            DocumentId = order.Id,
                            DocumentType = nameof(OutboundOrder),
                            Description = $"Outbound Picking - Ref: {order.DocumentNumber}",
                            UserId = userId,
                            CreatedDate = DateTime.UtcNow,
                            IsActive = true
                        });
                    }
                }

                warehouse.UsedCapacity -= totalVolumeToRemove;
                if (warehouse.UsedCapacity < 0) warehouse.UsedCapacity = 0;
                warehouse.UpdatedDate = DateTime.UtcNow;
                _warehouseRepository.Update(warehouse);

                order.Status = DocumentStatus.Completed;
                order.UpdatedDate = DateTime.UtcNow;
                order.ApprovedById = userId;

                _outboundRepository.Update(order);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                await NotifyWarehouseManagersAsync(order.WarehouseId, "Sevkiyat Tamamlandı", $"{order.DocumentNumber} numaralı sevkiyat fişinin fiziksel çıkışı tamamlandı.", NotificationType.Outbound, NotificationTargetType.OutboundOrder, order.Id);
                await CheckAndNotifyCriticalStockAsync(order.WarehouseId, productIds);
            }
            catch (DbUpdateConcurrencyException)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new ClientSideException("Sistem Meşgul: Aynı rafa veya stoğa başka bir operatör işlem yapıyor. Lütfen tekrar deneyin.");
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task CancelAsync(OutboundOrderCancelDto cancelDto)
        {
            var order = await _outboundRepository
                .Where(o => o.Id == cancelDto.OutboundOrderId)
                .SingleOrDefaultAsync();

            if (order == null) throw new ClientSideException("Belge bulunamadı.");
            ValidateRowLevelSecurity(order.WarehouseId);

            if (order.Status == DocumentStatus.Cancelled) throw new ClientSideException("Belge zaten iptal edilmiş.");
            if (order.Status == DocumentStatus.Completed) throw new ClientSideException("Tamamlanmış belgeler iptal edilemez. Lütfen Depo İade fişi oluşturun.");

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var reservations = await _reservationRepository.Where(r => r.OutboundOrderId == order.Id && r.IsActive).ToListAsync();

                foreach (var res in reservations)
                {
                    var stock = await _stockRepository.Where(s => s.WarehouseId == order.WarehouseId && s.ProductId == res.ProductId && s.ShelfId == res.ShelfId).SingleOrDefaultAsync();
                    if (stock != null)
                    {
                        stock.ReservedQuantity -= res.ReservedQuantity;
                        stock.Version = Guid.NewGuid();
                        _stockRepository.Update(stock);
                    }
                    _reservationRepository.Remove(res);
                }

                order.Status = DocumentStatus.Cancelled;
                order.UpdatedDate = DateTime.UtcNow;
                order.CancelledById = GetCurrentUserId();

                _outboundRepository.Update(order);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                await NotifyWarehouseManagersAsync(order.WarehouseId, "Mal Çıkış Talebi İptal Edildi", $"{order.DocumentNumber} numaralı çıkış talebi iptal edildi ve stoklar serbest bırakıldı.", NotificationType.Outbound, NotificationTargetType.OutboundOrder, order.Id);
            }
            catch (DbUpdateConcurrencyException)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new ClientSideException("Sistem Meşgul: Stoklara şu an başka bir operatör işlem yapıyor. Lütfen iptal işlemini tekrar deneyin.");
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        #endregion

        #region Read Operations

        public async Task<IEnumerable<OutboundOrderListDto>> GetAllAsync()
        {
            return await GetBaseQueryWithRls()
                .OrderByDescending(o => o.CreatedDate)
                .Select(o => new OutboundOrderListDto
                {
                    Id = o.Id,
                    DocumentNumber = o.DocumentNumber,
                    Destination = o.Destination,
                    WarehouseId = o.WarehouseId,
                    WarehouseName = o.Warehouse != null ? o.Warehouse.Name : "-",
                    MovementType = o.MovementType,
                    Status = o.Status,
                    CreatedDate = o.CreatedDate
                }).ToListAsync();
        }

        public async Task<PagedResult<OutboundOrderListDto>> GetPagedAsync(PaginationParams paginationParams)
        {
            var query = GetBaseQueryWithRls();
            var totalCount = await query.CountAsync();

            var data = await query
                .OrderByDescending(o => o.CreatedDate)
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .Select(o => new OutboundOrderListDto
                {
                    Id = o.Id,
                    DocumentNumber = o.DocumentNumber,
                    Destination = o.Destination,
                    WarehouseId = o.WarehouseId,
                    WarehouseName = o.Warehouse != null ? o.Warehouse.Name : "-",
                    MovementType = o.MovementType,
                    Status = o.Status,
                    CreatedDate = o.CreatedDate
                }).ToListAsync();

            return new PagedResult<OutboundOrderListDto>(data, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
        }

        public async Task<IEnumerable<OutboundOrderListDto>> GetAllByWarehouseIdAsync(Guid warehouseId)
        {
            ValidateRowLevelSecurity(warehouseId);
            return await _outboundRepository.Where(o => o.WarehouseId == warehouseId)
                .OrderByDescending(o => o.CreatedDate)
                .Select(o => new OutboundOrderListDto
                {
                    Id = o.Id,
                    DocumentNumber = o.DocumentNumber,
                    Destination = o.Destination,
                    WarehouseId = o.WarehouseId,
                    WarehouseName = o.Warehouse != null ? o.Warehouse.Name : "-",
                    MovementType = o.MovementType,
                    Status = o.Status,
                    CreatedDate = o.CreatedDate
                }).ToListAsync();
        }

        public async Task<OutboundOrderDetailDto> GetByIdAsync(Guid id)
        {
            var order = await _outboundRepository.Where(o => o.Id == id)
                .Select(o => new OutboundOrderDetailDto
                {
                    Id = o.Id,
                    DocumentNumber = o.DocumentNumber,
                    Destination = o.Destination,
                    WarehouseId = o.WarehouseId,
                    WarehouseName = o.Warehouse != null ? o.Warehouse.Name : "-",
                    MovementType = o.MovementType,
                    Status = o.Status,
                    CreatedDate = o.CreatedDate,
                    Description = o.Description,

                    CreatedByName = o.CreatedBy != null ? o.CreatedBy.FirstName + " " + o.CreatedBy.LastName : "-",
                    ApprovedByName = o.ApprovedBy != null ? o.ApprovedBy.FirstName + " " + o.ApprovedBy.LastName : null,
                    CancelledByName = o.CancelledBy != null ? o.CancelledBy.FirstName + " " + o.CancelledBy.LastName : null,

                    Lines = o.Lines.Select(l => new OutboundOrderLineDto
                    {
                        Id = l.Id,
                        ProductId = l.ProductId,
                        ProductName = l.Product != null ? l.Product.Name : "-",
                        ProductCode = l.Product != null ? l.Product.Sku : "-",
                        RequestedQuantity = l.RequestedQuantity,
                        PickedQuantity = l.PickedQuantity,
                        PickedShelf = "",

                        Allocations = o.Reservations.Where(r => r.ProductId == l.ProductId && r.IsActive)
                            .Select(r => new OutboundAllocationDto
                            {
                                ShelfId = r.ShelfId,
                                ShelfName = r.Shelf != null ? r.Shelf.ShelfNumber : "Bilinmiyor",
                                Quantity = r.ReservedQuantity
                            }).ToList()
                    }).ToList()
                }).SingleOrDefaultAsync();

            if (order == null) throw new ClientSideException("Belge bulunamadı.");
            ValidateRowLevelSecurity(order.WarehouseId);

            if (order.Status == DocumentStatus.Completed)
            {
                var movements = await _stockMovementRepository
                    .Where(sm => sm.DocumentId == id && sm.DocumentType == nameof(OutboundOrder))
                    .Select(sm => new { sm.ProductId, sm.Shelf.ShelfNumber, sm.Quantity })
                    .ToListAsync();

                foreach (var lineDto in order.Lines)
                {
                    var lineMovements = movements.Where(m => m.ProductId == lineDto.ProductId).ToList();
                    if (lineMovements.Any())
                    {
                        lineDto.PickedShelf = string.Join(" | ", lineMovements.Select(m => $"{m.ShelfNumber} ({m.Quantity} Adet)"));
                    }
                }
            }
            return order;
        }
        #endregion

        #region Private Helpers

        private IQueryable<OutboundOrder> GetBaseQueryWithRls()
        {
            var query = _outboundRepository.GetAll();
            var currentUserRole = GetCurrentUserRole();

            if (currentUserRole != UserRole.SuperAdmin.ToString())
            {
                var currentWarehouseId = GetCurrentWarehouseId();
                query = query.Where(o => o.WarehouseId == currentWarehouseId);
            }
            return query;
        }

        private void ValidateRowLevelSecurity(Guid requestedWarehouseId)
        {
            var currentUserRole = GetCurrentUserRole();
            if (currentUserRole != UserRole.SuperAdmin.ToString())
            {
                var currentWarehouseId = GetCurrentWarehouseId();
                if (currentWarehouseId == null || currentWarehouseId != requestedWarehouseId)
                    throw new ClientSideException("Başka bir deponun fişleri üzerinde işlem yapma yetkiniz bulunmamaktadır.");
            }
        }

        private string GetCurrentUserRole()
        {
            return _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        private Guid GetCurrentUserId()
        {
            var userIdString = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
                throw new UnauthorizedAccessException("Kullanıcı kimliği doğrulanamadı.");
            return userId;
        }

        private Guid? GetCurrentWarehouseId()
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirst("WarehouseId");
            if (claim != null && Guid.TryParse(claim.Value, out var warehouseId))
                return warehouseId;
            return null;
        }

        private async Task<string> GenerateDocumentNumberAsync()
        {
            var today = DateTime.UtcNow.Date;
            var lastOrder = await _outboundRepository
                .Where(o => o.CreatedDate >= today)
                .OrderByDescending(o => o.DocumentNumber)
                .FirstOrDefaultAsync();

            if (lastOrder == null) return $"OUT-{today:yyyyMMdd}-000001";
            var lastNumberStr = lastOrder.DocumentNumber.Substring(lastOrder.DocumentNumber.Length - 6);
            if (int.TryParse(lastNumberStr, out int lastNumber)) return $"OUT-{today:yyyyMMdd}-{(lastNumber + 1):D6}";

            var count = await _outboundRepository.Where(o => o.CreatedDate >= today).CountAsync();
            return $"OUT-{today:yyyyMMdd}-{(count + 1):D6}";
        }

        private async Task NotifyWarehouseManagersAsync(Guid warehouseId, string title, string message, NotificationType type, NotificationTargetType targetType, Guid targetId)
        {
            var targetUsers = await _userRepository
                .Where(u => u.IsActive && (u.Role == UserRole.SuperAdmin || (u.Role == UserRole.WarehouseManager && u.WarehouseId == warehouseId)))
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

        private async Task CheckAndNotifyCriticalStockAsync(Guid warehouseId, List<Guid> productIds)
        {
            var totalStocks = await _stockRepository
                .Where(s => s.WarehouseId == warehouseId && productIds.Contains(s.ProductId) && s.IsActive)
                .Select(s => new { s.ProductId, s.Product.Name, s.Product.CriticalLevel, s.Quantity })
                .GroupBy(s => new { s.ProductId, s.Name, s.CriticalLevel })
                .Select(g => new
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    CriticalLevel = g.Key.CriticalLevel,
                    TotalQuantity = g.Sum(x => x.Quantity)
                }).ToListAsync();

            foreach (var stock in totalStocks)
            {
                if (stock.TotalQuantity <= stock.CriticalLevel)
                {
                    await NotifyWarehouseManagersAsync(warehouseId, "KRİTİK STOK ALARMI!", $"{stock.ProductName} ürününün stok seviyesi kritik sınıra düştü! (Kalan Toplam Stok: {stock.TotalQuantity}, Limit: {stock.CriticalLevel}).", NotificationType.CriticalStock, NotificationTargetType.Product, stock.ProductId);
                }
            }
        }

        #endregion
    }
}