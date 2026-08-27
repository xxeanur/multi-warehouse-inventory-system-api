using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MultiWarehouse.Entity.Entities.Definitions;
using MultiWarehouse.Entity.Entities.Documents;
using MultiWarehouse.Entity.Entities.Identity;
using MultiWarehouse.Entity.Entities.Inventory;
using MultiWarehouse.Entity.Enums.Common;
using MultiWarehouse.Entity.Enums.Document;
using MultiWarehouse.Entity.Enums.Inventory;
using MultiWarehouse.Entity.Enums.User;
using MultiWarehouse.Service.Exceptions;
using MultiWarehouse.Service.Repositories.Interfaces;
using MultiWarehouse.Service.Services.Interfaces.Documents;
using MultiWarehouse.Service.Services.Interfaces.Notification;
using MultiWarehouse.Shared.DTOs.DocumentDtos.TransferDtos;
using MultiWarehouse.Shared.DTOs.NotificationDtos;
using MultiWarehouse.Shared.Pagination;
using System.Security.Claims;

namespace MultiWarehouse.Service.Services.Implementations.Documents
{
    public class TransferOrderService : ITransferOrderService
    {
        #region Dependencies

        private readonly IGenericRepository<TransferOrder> _transferRepository;
        private readonly IGenericRepository<TransferOrderReservation> _reservationRepository;
        private readonly IGenericRepository<Stock> _stockRepository;
        private readonly IGenericRepository<Warehouse> _warehouseRepository;
        private readonly IGenericRepository<Shelf> _shelfRepository;
        private readonly IGenericRepository<Product> _productRepository;
        private readonly IGenericRepository<StockMovement> _stockMovementRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TransferOrderService(
            IGenericRepository<TransferOrder> transferRepository,
            IGenericRepository<TransferOrderReservation> reservationRepository,
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
            _transferRepository = transferRepository;
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

        public async Task<Guid> CreateAsync(TransferOrderCreateDto createDto)
        {
            ValidateRowLevelSecurity(createDto.SourceWarehouseId);

            if (createDto.SourceWarehouseId == createDto.TargetWarehouseId)
                throw new ClientSideException("Kaynak ve Hedef depo aynı olamaz!");

            var sourceWarehouseExists = await _warehouseRepository.Where(w => w.Id == createDto.SourceWarehouseId && w.IsActive).AnyAsync();
            var targetWarehouseExists = await _warehouseRepository.Where(w => w.Id == createDto.TargetWarehouseId && w.IsActive).AnyAsync();
            if (!sourceWarehouseExists || !targetWarehouseExists)
                throw new ClientSideException("Kaynak veya hedef depo sistemde bulunamadı ya da aktif değil.");

            var duplicateCheck = createDto.Lines.GroupBy(l => l.ProductId).Any(g => g.Count() > 1);
            if (duplicateCheck)
                throw new ClientSideException("Bir ürün transfer fişinde yalnızca tek bir satırda yer alabilir.");

            foreach (var line in createDto.Lines)
            {
                if (line.Quantity <= 0)
                    throw new ClientSideException("Transfer miktarı sıfırdan büyük olmalıdır.");
            }

            var productIds = createDto.Lines.Select(l => l.ProductId).ToList();
            var existingProductsCount = await _productRepository.Where(p => productIds.Contains(p.Id) && p.IsActive).CountAsync();
            if (existingProductsCount != productIds.Count)
                throw new ClientSideException("Seçilen ürünlerden bazıları sistemde bulunamadı veya pasif durumda.");

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var allAvailableStocks = await _stockRepository
                    .Where(s => s.WarehouseId == createDto.SourceWarehouseId && productIds.Contains(s.ProductId) && s.IsActive)
                    .ToListAsync();

                var orderId = Guid.NewGuid();
                var order = new TransferOrder
                {
                    Id = orderId,
                    DocumentNumber = await GenerateDocumentNumberAsync(),
                    SourceWarehouseId = createDto.SourceWarehouseId,
                    TargetWarehouseId = createDto.TargetWarehouseId,
                    Description = createDto.Description,
                    Status = DocumentStatus.Pending,
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true,
                    CreatedById = GetCurrentUserId()
                };

                foreach (var lineDto in createDto.Lines)
                {
                    order.Lines.Add(new TransferOrderLine
                    {
                        ProductId = lineDto.ProductId,
                        ExpectedQuantity = lineDto.Quantity,
                        DispatchedQuantity = 0,
                        ReceivedQuantity = 0,
                        CreatedDate = DateTime.UtcNow,
                        IsActive = true
                    });

                    var productStocks = allAvailableStocks.Where(s => s.ProductId == lineDto.ProductId).ToList();
                    var totalAvailable = productStocks.Sum(s => s.Quantity - s.ReservedQuantity);

                    if (totalAvailable < lineDto.Quantity)
                        throw new ClientSideException($"Yetersiz Stok! Kaynak depoda seçilen ürün için kullanılabilir sadece {totalAvailable} adet var.");

                    int remainingToReserve = lineDto.Quantity;

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

                            await _reservationRepository.AddAsync(new TransferOrderReservation
                            {
                                TransferOrderId = orderId,
                                ProductId = lineDto.ProductId,
                                SourceShelfId = stock.ShelfId,
                                ReservedQuantity = reserveAmount,
                                CreatedDate = DateTime.UtcNow,
                                IsActive = true
                            });

                            remainingToReserve -= reserveAmount;
                        }
                    }
                }

                await _transferRepository.AddAsync(order);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                await NotifyWarehouseManagersAsync(order.SourceWarehouseId, "Yeni Transfer Talebi", $"{order.DocumentNumber} numaralı yeni bir transfer talebi oluşturuldu.", NotificationType.Transfer, NotificationTargetType.TransferOrder, order.Id);

                return order.Id;
            }
            catch (DbUpdateConcurrencyException)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new ClientSideException("Sistem Meşgul: Seçtiğiniz stoklara şu an başka bir operatör işlem yapıyor.");
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task DispatchAsync(TransferOrderDispatchDto dispatchDto)
        {
            var userId = GetCurrentUserId();
            var order = await _transferRepository.Where(o => o.Id == dispatchDto.TransferOrderId).Include(o => o.Lines).SingleOrDefaultAsync();

            if (order == null) throw new ClientSideException("Belge bulunamadı.");
            ValidateRowLevelSecurity(order.SourceWarehouseId);

            if (order.Status != DocumentStatus.Pending)
                throw new ClientSideException("Sadece Beklemede (Pending) olan transferler yola çıkarılabilir.");

            var aggregatedDispatched = dispatchDto.DispatchedLines
                .GroupBy(d => new { d.TransferOrderLineId, d.SourceShelfId })
                .Select(g => new { g.Key.TransferOrderLineId, g.Key.SourceShelfId, Quantity = g.Sum(x => x.Quantity) })
                .ToList();

            var warehouse = await _warehouseRepository.Where(w => w.Id == order.SourceWarehouseId).SingleOrDefaultAsync();
            var productIds = order.Lines.Select(l => l.ProductId).Distinct().ToList();
            var shelfIds = aggregatedDispatched.Select(p => p.SourceShelfId).Distinct().ToList();

            var productsDict = await _productRepository.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);
            var shelvesDict = await _shelfRepository.Where(s => shelfIds.Contains(s.Id)).Include(s => s.WarehouseZone).ToDictionaryAsync(s => s.Id);
            var stocksList = await _stockRepository.Where(s => s.WarehouseId == order.SourceWarehouseId && productIds.Contains(s.ProductId) && s.IsActive).ToListAsync();

            var reservations = await _reservationRepository.Where(r => r.TransferOrderId == order.Id && r.IsActive).ToListAsync();

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var dispatchLookup = aggregatedDispatched.ToLookup(a => a.TransferOrderLineId);
                double totalVolumeToRemove = 0;

                foreach (var line in order.Lines)
                {
                    var lineInputs = dispatchLookup[line.Id].ToList();
                    var lineReservations = reservations.Where(r => r.ProductId == line.ProductId).ToList();
                    var totalDispatched = lineInputs.Sum(a => a.Quantity);

                    if (totalDispatched != line.ExpectedQuantity)
                        throw new ClientSideException($"Kısmi transfer kapalıdır! Beklenen: {line.ExpectedQuantity}, Yola Çıkarılan: {totalDispatched}");

                    foreach (var input in lineInputs)
                    {
                        var correspondingReservation = lineReservations.FirstOrDefault(r => r.SourceShelfId == input.SourceShelfId);
                        if (correspondingReservation == null || correspondingReservation.ReservedQuantity < input.Quantity)
                        {
                            var expectedShelves = string.Join(", ", lineReservations.Select(r => $"{shelvesDict.GetValueOrDefault(r.SourceShelfId)?.ShelfNumber} ({r.ReservedQuantity} adet)"));
                            throw new ClientSideException($"Sıkı Tahsis İhlali! Sistem bu ürün için şu rafları tahsis etti: {expectedShelves}. Lütfen o raflardan toplayınız.");
                        }
                    }

                    line.DispatchedQuantity = totalDispatched;

                    if (productsDict.TryGetValue(line.ProductId, out var product))
                    {
                        double unitVolume = product.Width * product.Height * product.Depth;
                        totalVolumeToRemove += (unitVolume * totalDispatched);
                    }

                    foreach (var input in lineInputs)
                    {
                        var stock = stocksList.FirstOrDefault(s => s.ShelfId == input.SourceShelfId && s.ProductId == line.ProductId);
                        if (stock == null) throw new ClientSideException("Kaynak stok bulunamadı.");

                        var reservation = reservations.FirstOrDefault(r => r.SourceShelfId == input.SourceShelfId && r.ProductId == line.ProductId);

                        stock.Quantity -= input.Quantity;
                        stock.ReservedQuantity -= input.Quantity;
                        stock.UpdatedDate = DateTime.UtcNow;
                        stock.Version = Guid.NewGuid();
                        _stockRepository.Update(stock);

                        if (reservation != null) _reservationRepository.Remove(reservation);

                        if (shelvesDict.TryGetValue(input.SourceShelfId, out var shelf))
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
                            WarehouseId = order.SourceWarehouseId,
                            ProductId = line.ProductId,
                            ShelfId = input.SourceShelfId,
                            MovementType = MovementType.TransferOut,
                            Quantity = input.Quantity,
                            DocumentId = order.Id,
                            DocumentType = nameof(TransferOrder),
                            Description = $"Transfer Dispatch - Hedef: {order.TargetWarehouseId}",
                            UserId = userId,
                            CreatedDate = DateTime.UtcNow,
                            IsActive = true
                        });
                    }
                }

                if (warehouse != null)
                {
                    warehouse.UsedCapacity -= totalVolumeToRemove;
                    if (warehouse.UsedCapacity < 0) warehouse.UsedCapacity = 0;
                    warehouse.UpdatedDate = DateTime.UtcNow;
                    _warehouseRepository.Update(warehouse);
                }

                order.Status = DocumentStatus.InTransit;
                order.UpdatedDate = DateTime.UtcNow;
                order.DispatchedById = userId;

                _transferRepository.Update(order);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                await NotifyWarehouseManagersAsync(order.TargetWarehouseId, "Transfer Yola Çıktı", $"{order.DocumentNumber} numaralı transfer yola çıkmıştır.", NotificationType.Transfer, NotificationTargetType.TransferOrder, order.Id);
            }
            catch (DbUpdateConcurrencyException)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new ClientSideException("Sistem Meşgul: Aynı rafa başka bir operatör işlem yapıyor.");
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task ReceiveAsync(TransferOrderReceiveDto receiveDto)
        {
            var userId = GetCurrentUserId();
            var order = await _transferRepository.Where(o => o.Id == receiveDto.TransferOrderId).Include(o => o.Lines).SingleOrDefaultAsync();

            if (order == null) throw new ClientSideException("Belge bulunamadı.");
            ValidateRowLevelSecurity(order.TargetWarehouseId);

            if (order.Status != DocumentStatus.InTransit)
                throw new ClientSideException("Sadece Yoldaki (InTransit) transferler kapıda teslim alınabilir.");

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                foreach (var line in order.Lines)
                {
                    var input = receiveDto.ReceivedLines.FirstOrDefault(r => r.TransferOrderLineId == line.Id);
                    int receivedQty = input?.Quantity ?? 0;

                    if (receivedQty != line.DispatchedQuantity)
                        throw new ClientSideException($"Kısmi teslim alma kapalıdır! Yola Çıkan: {line.DispatchedQuantity}, Kapıda Sayılan: {receivedQty}");

                    line.ReceivedQuantity = receivedQty;
                }

                order.Status = DocumentStatus.Approved;
                order.UpdatedDate = DateTime.UtcNow;
                order.ReceivedById = userId;

                _transferRepository.Update(order);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                await NotifyWarehouseManagersAsync(order.TargetWarehouseId, "Transfer Kapıda Teslim Alındı", $"{order.DocumentNumber} numaralı transfer hedef depo tarafından kapıda sayılarak onaylandı. Raflama (Putaway) bekleniyor.", NotificationType.Transfer, NotificationTargetType.TransferOrder, order.Id);
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task CancelAsync(TransferOrderCancelDto cancelDto)
        {
            var order = await _transferRepository.Where(o => o.Id == cancelDto.TransferOrderId).SingleOrDefaultAsync();

            if (order == null) throw new ClientSideException("Belge bulunamadı.");
            ValidateRowLevelSecurity(order.SourceWarehouseId);

            if (order.Status == DocumentStatus.Cancelled) throw new ClientSideException("Belge zaten iptal edilmiş.");
            if (order.Status == DocumentStatus.Completed || order.Status == DocumentStatus.Approved)
                throw new ClientSideException("Teslim alınmış transferler iptal edilemez.");
            if (order.Status == DocumentStatus.InTransit)
                throw new ClientSideException("Yola çıkmış (InTransit) transfer iptal edilemez.");

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var reservations = await _reservationRepository.Where(r => r.TransferOrderId == order.Id && r.IsActive).ToListAsync();

                foreach (var res in reservations)
                {
                    var stock = await _stockRepository.Where(s => s.WarehouseId == order.SourceWarehouseId && s.ProductId == res.ProductId && s.ShelfId == res.SourceShelfId).SingleOrDefaultAsync();
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

                _transferRepository.Update(order);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                await NotifyWarehouseManagersAsync(order.SourceWarehouseId, "Transfer İptal Edildi", $"{order.DocumentNumber} numaralı transfer iptal edildi.", NotificationType.Transfer, NotificationTargetType.TransferOrder, order.Id);
            }
            catch (DbUpdateConcurrencyException)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new ClientSideException("Sistem Meşgul: Stoklara başka bir operatör işlem yapıyor.");
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        #endregion

        #region Read Operations

        public async Task<IEnumerable<TransferOrderListDto>> GetAllAsync()
        {
            return await GetBaseQueryWithRls()
                .OrderByDescending(o => o.CreatedDate)
                .Select(o => new TransferOrderListDto
                {
                    Id = o.Id,
                    DocumentNumber = o.DocumentNumber,
                    SourceWarehouseId = o.SourceWarehouseId,
                    TargetWarehouseId = o.TargetWarehouseId,
                    SourceWarehouseName = o.SourceWarehouse != null ? o.SourceWarehouse.Name : "-",
                    TargetWarehouseName = o.TargetWarehouse != null ? o.TargetWarehouse.Name : "-",
                    Status = o.Status,
                    CreatedDate = o.CreatedDate
                }).ToListAsync();
        }

        public async Task<PagedResult<TransferOrderListDto>> GetPagedAsync(PaginationParams paginationParams)
        {
            var query = GetBaseQueryWithRls();
            var totalCount = await query.CountAsync();

            var data = await query
                .OrderByDescending(o => o.CreatedDate)
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .Select(o => new TransferOrderListDto
                {
                    Id = o.Id,
                    DocumentNumber = o.DocumentNumber,
                    SourceWarehouseId = o.SourceWarehouseId,
                    TargetWarehouseId = o.TargetWarehouseId,
                    SourceWarehouseName = o.SourceWarehouse != null ? o.SourceWarehouse.Name : "-",
                    TargetWarehouseName = o.TargetWarehouse != null ? o.TargetWarehouse.Name : "-",
                    Status = o.Status,
                    CreatedDate = o.CreatedDate
                }).ToListAsync();

            return new PagedResult<TransferOrderListDto>(data, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
        }

        public async Task<IEnumerable<TransferOrderListDto>> GetAllBySourceWarehouseIdAsync(Guid warehouseId)
        {
            ValidateRowLevelSecurity(warehouseId);
            return await _transferRepository.Where(o => o.SourceWarehouseId == warehouseId)
                .OrderByDescending(o => o.CreatedDate)
                .Select(o => new TransferOrderListDto
                {
                    Id = o.Id,
                    DocumentNumber = o.DocumentNumber,
                    SourceWarehouseId = o.SourceWarehouseId,
                    TargetWarehouseId = o.TargetWarehouseId,
                    SourceWarehouseName = o.SourceWarehouse != null ? o.SourceWarehouse.Name : "-",
                    TargetWarehouseName = o.TargetWarehouse != null ? o.TargetWarehouse.Name : "-",
                    Status = o.Status,
                    CreatedDate = o.CreatedDate
                }).ToListAsync();
        }

        public async Task<IEnumerable<TransferOrderListDto>> GetAllByTargetWarehouseIdAsync(Guid warehouseId)
        {
            ValidateRowLevelSecurity(warehouseId);
            return await _transferRepository.Where(o => o.TargetWarehouseId == warehouseId)
                .OrderByDescending(o => o.CreatedDate)
                .Select(o => new TransferOrderListDto
                {
                    Id = o.Id,
                    DocumentNumber = o.DocumentNumber,
                    SourceWarehouseId = o.SourceWarehouseId,
                    TargetWarehouseId = o.TargetWarehouseId,
                    SourceWarehouseName = o.SourceWarehouse != null ? o.SourceWarehouse.Name : "-",
                    TargetWarehouseName = o.TargetWarehouse != null ? o.TargetWarehouse.Name : "-",
                    Status = o.Status,
                    CreatedDate = o.CreatedDate
                }).ToListAsync();
        }

        public async Task<TransferOrderDetailDto> GetByIdAsync(Guid id)
        {
            var order = await _transferRepository.Where(o => o.Id == id)
                .Select(o => new TransferOrderDetailDto
                {
                    Id = o.Id,
                    DocumentNumber = o.DocumentNumber,
                    SourceWarehouseId = o.SourceWarehouseId,
                    TargetWarehouseId = o.TargetWarehouseId,
                    SourceWarehouseName = o.SourceWarehouse != null ? o.SourceWarehouse.Name : "-",
                    TargetWarehouseName = o.TargetWarehouse != null ? o.TargetWarehouse.Name : "-",
                    Status = o.Status,
                    CreatedDate = o.CreatedDate,
                    Description = o.Description,

                    CreatedByName = o.CreatedBy != null ? o.CreatedBy.FirstName + " " + o.CreatedBy.LastName : "-",
                    DispatchedByName = o.DispatchedBy != null ? o.DispatchedBy.FirstName + " " + o.DispatchedBy.LastName : null,
                    ReceivedByName = o.ReceivedBy != null ? o.ReceivedBy.FirstName + " " + o.ReceivedBy.LastName : null,
                    CancelledByName = o.CancelledBy != null ? o.CancelledBy.FirstName + " " + o.CancelledBy.LastName : null,

                    Lines = o.Lines.Select(l => new TransferOrderLineDto
                    {
                        Id = l.Id,
                        ProductId = l.ProductId,
                        ProductName = l.Product != null ? l.Product.Name : "-",
                        ProductCode = l.Product != null ? l.Product.Sku : "-",
                        ExpectedQuantity = l.ExpectedQuantity,
                        DispatchedQuantity = l.DispatchedQuantity,
                        ReceivedQuantity = l.ReceivedQuantity,

                        Allocations = o.Reservations.Where(r => r.ProductId == l.ProductId && r.IsActive)
                            .Select(r => new TransferAllocationDto
                            {
                                SourceShelfId = r.SourceShelfId,
                                SourceShelfName = r.SourceShelf != null ? r.SourceShelf.ShelfNumber : "Bilinmiyor",
                                Quantity = r.ReservedQuantity
                            }).ToList()
                    }).ToList()
                }).SingleOrDefaultAsync();

            if (order == null) throw new ClientSideException("Belge bulunamadı.");

            var currentUserRole = GetCurrentUserRole();
            if (currentUserRole != UserRole.SuperAdmin.ToString())
            {
                var currentWarehouseId = GetCurrentWarehouseId();
                if (currentWarehouseId != order.SourceWarehouseId && currentWarehouseId != order.TargetWarehouseId)
                    throw new ClientSideException("Bu transfer belgesini görüntüleme yetkiniz yok.");
            }

            return order;
        }

        #endregion

        #region Private Helpers

        private IQueryable<TransferOrder> GetBaseQueryWithRls()
        {
            var query = _transferRepository.GetAll();
            var currentUserRole = GetCurrentUserRole();

            if (currentUserRole != UserRole.SuperAdmin.ToString())
            {
                var currentWarehouseId = GetCurrentWarehouseId();
                query = query.Where(o => o.SourceWarehouseId == currentWarehouseId || o.TargetWarehouseId == currentWarehouseId);
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
                    throw new ClientSideException("Başka bir deponun transfer fişleri üzerinde işlem yapma yetkiniz bulunmamaktadır.");
            }
        }

        private string GetCurrentUserRole()
        {
            return _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        private Guid? GetCurrentWarehouseId()
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirst("WarehouseId");
            if (claim != null && Guid.TryParse(claim.Value, out var warehouseId))
                return warehouseId;
            return null;
        }

        private Guid GetCurrentUserId()
        {
            var userIdString = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
                throw new UnauthorizedAccessException("Kullanıcı kimliği doğrulanamadı.");
            return userId;
        }

        private async Task<string> GenerateDocumentNumberAsync()
        {
            var today = DateTime.UtcNow.Date;
            var lastOrder = await _transferRepository
                .Where(o => o.CreatedDate >= today)
                .OrderByDescending(o => o.DocumentNumber)
                .FirstOrDefaultAsync();

            if (lastOrder == null) return $"TRA-{today:yyyyMMdd}-000001";
            var lastNumberStr = lastOrder.DocumentNumber.Substring(lastOrder.DocumentNumber.Length - 6);
            if (int.TryParse(lastNumberStr, out int lastNumber)) return $"TRA-{today:yyyyMMdd}-{(lastNumber + 1):D6}";

            var count = await _transferRepository.Where(o => o.CreatedDate >= today).CountAsync();
            return $"TRA-{today:yyyyMMdd}-{(count + 1):D6}";
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

        #endregion
    }
}