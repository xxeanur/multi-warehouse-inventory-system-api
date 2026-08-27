using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MultiWarehouse.Entity.Entities.Definitions;
using MultiWarehouse.Entity.Entities.Documents;
using MultiWarehouse.Entity.Entities.Inventory;
using MultiWarehouse.Entity.Enums.Document;
using MultiWarehouse.Entity.Enums.User;
using MultiWarehouse.Service.Exceptions;
using MultiWarehouse.Service.Repositories.Interfaces;
using MultiWarehouse.Service.Services.Interfaces.Inventory;
using MultiWarehouse.Shared.DTOs.PutawayDtos;
using System.Security.Claims;

namespace MultiWarehouse.Service.Services.Implementations.Inventory
{
    public class PutawayService : IPutawayService
    {
        private readonly IGenericRepository<InboundOrder> _inboundOrderRepository;
        private readonly IGenericRepository<TransferOrder> _transferOrderRepository;
        private readonly IGenericRepository<Warehouse> _warehouseRepository;
        private readonly IGenericRepository<Product> _productRepository;
        private readonly IGenericRepository<Shelf> _shelfRepository;
        private readonly IGenericRepository<Stock> _stockRepository;
        private readonly IGenericRepository<StockMovement> _stockMovementRepository;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PutawayService(
            IGenericRepository<InboundOrder> inboundOrderRepository,
            IGenericRepository<TransferOrder> transferOrderRepository,
            IGenericRepository<Warehouse> warehouseRepository,
            IGenericRepository<Product> productRepository,
            IGenericRepository<Shelf> shelfRepository,
            IGenericRepository<Stock> stockRepository,
            IGenericRepository<StockMovement> stockMovementRepository,
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor)
        {
            _inboundOrderRepository = inboundOrderRepository;
            _transferOrderRepository = transferOrderRepository;
            _warehouseRepository = warehouseRepository;
            _productRepository = productRepository;
            _shelfRepository = shelfRepository;
            _stockRepository = stockRepository;
            _stockMovementRepository = stockMovementRepository;
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        #region Read Operations (Birleştirilmiş Listeler)

        public async Task<IEnumerable<PutawayListDto>> GetPendingPutawaysAsync(Guid warehouseId)
        {
            ValidateRowLevelSecurity(warehouseId);
            var result = new List<PutawayListDto>();

            var inbounds = await _inboundOrderRepository
                .Where(i => i.WarehouseId == warehouseId && i.Status == DocumentStatus.Approved)
                .Include(i => i.Supplier)
                .Include(i => i.SourceTransferOrder)
                .Select(i => new PutawayListDto
                {
                    DocumentId = i.Id,
                    DocumentNumber = i.DocumentNumber,
                    DocumentType = "Inbound",
                    MovementTypeName = i.MovementType == MultiWarehouse.Entity.Enums.Inventory.MovementType.TransferIn
                                        ? "Transfer Girişi"
                                        : "Mal Kabul",

                    SourceName = i.SourceTransferOrder != null
                                 ? $"Transfer (Ref: {i.SourceTransferOrder.DocumentNumber})"
                                 : (i.Supplier != null ? i.Supplier.CompanyName : "Bilinmeyen Tedarikçi"),
                    CreatedDate = i.CreatedDate
                }).ToListAsync();

            var transfers = await _transferOrderRepository
                .Where(t => t.TargetWarehouseId == warehouseId && t.Status == DocumentStatus.Approved)
                .Include(t => t.SourceWarehouse)
                .Select(t => new PutawayListDto
                {
                    DocumentId = t.Id,
                    DocumentNumber = t.DocumentNumber,
                    DocumentType = "Transfer",
                    MovementTypeName = "Depolar Arası Transfer",
                    SourceName = t.SourceWarehouse != null ? t.SourceWarehouse.Name : "Bilinmeyen Kaynak Depo",
                    CreatedDate = t.CreatedDate
                }).ToListAsync();

            result.AddRange(inbounds);
            result.AddRange(transfers);

            return result.OrderBy(r => r.CreatedDate);
        }

        public async Task<PutawayDetailDto> GetPutawayDetailAsync(Guid documentId, string documentType)
        {
            if (documentType == "Inbound")
            {
                var inbound = await _inboundOrderRepository.Where(i => i.Id == documentId).Include(i => i.Lines).ThenInclude(l => l.Product).SingleOrDefaultAsync();
                if (inbound == null) throw new ClientSideException("Inbound belgesi bulunamadı.");

                return new PutawayDetailDto
                {
                    DocumentId = inbound.Id,
                    DocumentNumber = inbound.DocumentNumber,
                    DocumentType = "Inbound",
                    Lines = inbound.Lines.Select(l => new PutawayDetailLineDto
                    {
                        DocumentLineId = l.Id,
                        ProductId = l.ProductId,
                        ProductName = l.Product?.Name ?? "-",
                        ProductCode = l.Product?.Sku ?? "-",
                        QuantityToPlace = l.ExpectedQuantity
                    }).ToList()
                };
            }
            else if (documentType == "Transfer")
            {
                var transfer = await _transferOrderRepository.Where(t => t.Id == documentId).Include(t => t.Lines).ThenInclude(l => l.Product).SingleOrDefaultAsync();
                if (transfer == null) throw new ClientSideException("Transfer belgesi bulunamadı.");

                return new PutawayDetailDto
                {
                    DocumentId = transfer.Id,
                    DocumentNumber = transfer.DocumentNumber,
                    DocumentType = "Transfer",
                    Lines = transfer.Lines.Select(l => new PutawayDetailLineDto
                    {
                        DocumentLineId = l.Id,
                        ProductId = l.ProductId,
                        ProductName = l.Product?.Name ?? "-",
                        ProductCode = l.Product?.Sku ?? "-",
                        QuantityToPlace = l.ReceivedQuantity
                    }).ToList()
                };
            }

            throw new ClientSideException("Bilinmeyen belge türü.");
        }

        #endregion

        #region Write Operations (Ortak Core Logic)

        public async Task<bool> ExecutePutawayAsync(PutawayRequestDto requestDto)
        {
            var userId = GetCurrentUserId();
            ValidateRowLevelSecurity(requestDto.WarehouseId);

            var warehouse = await _warehouseRepository.Where(w => w.Id == requestDto.WarehouseId).FirstOrDefaultAsync();
            if (warehouse == null) throw new ClientSideException("Hedef depo bulunamadı.");

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                if (requestDto.DocumentType == "Inbound")
                {
                    await ProcessInboundPutaway(requestDto, warehouse, userId);
                }
                else if (requestDto.DocumentType == "Transfer")
                {
                    await ProcessTransferPutaway(requestDto, warehouse, userId);
                }
                else
                {
                    throw new ClientSideException("Bilinmeyen belge türü.");
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return true;
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

        private async Task ProcessInboundPutaway(PutawayRequestDto request, Warehouse warehouse, Guid userId)
        {
            var order = await _inboundOrderRepository.Where(i => i.Id == request.DocumentId).Include(i => i.Lines).ThenInclude(l => l.Product).FirstOrDefaultAsync();
            if (order == null) throw new ClientSideException("Fiş bulunamadı.");
            if (order.Status != DocumentStatus.Approved) throw new ClientSideException("Sadece kapıda sayılmış (Approved) fişler raflanabilir.");

            var groupedPlacements = request.PlacedLines.GroupBy(p => p.DocumentLineId).ToList();
            foreach (var line in order.Lines)
            {
                var linePlacements = groupedPlacements.FirstOrDefault(g => g.Key == line.Id);
                var totalPlaced = linePlacements?.Sum(p => p.Quantity) ?? 0;

                if (totalPlaced != line.ExpectedQuantity)
                    throw new ClientSideException($"{line.Product?.Name} ürünü için kapıda {line.ExpectedQuantity} adet sayıldı, ancak raflara {totalPlaced} adet atanıyor!");

                line.ReceivedQuantity = totalPlaced;
            }

            await ApplyStockAndCapacityUpdates(request, warehouse, order.Id, nameof(InboundOrder), $"Inbound Putaway - Ref: {order.DocumentNumber}", MultiWarehouse.Entity.Enums.Inventory.MovementType.Inbound, userId);

            order.Status = DocumentStatus.Completed;
            order.UpdatedDate = DateTime.UtcNow;
            _inboundOrderRepository.Update(order);

            if (order.SourceTransferOrderId.HasValue)
            {
                var transferOrder = await _transferOrderRepository
                    .Where(t => t.Id == order.SourceTransferOrderId.Value)
                    .FirstOrDefaultAsync();

                if (transferOrder != null)
                {
                    transferOrder.Status = DocumentStatus.Completed;
                    transferOrder.UpdatedDate = DateTime.UtcNow;
                    _transferOrderRepository.Update(transferOrder);
                }
            }
        }

        private async Task ProcessTransferPutaway(PutawayRequestDto request, Warehouse warehouse, Guid userId)
        {
            var order = await _transferOrderRepository.Where(t => t.Id == request.DocumentId).Include(t => t.Lines).ThenInclude(l => l.Product).FirstOrDefaultAsync();
            if (order == null) throw new ClientSideException("Transfer fişi bulunamadı.");
            if (order.Status != DocumentStatus.Approved) throw new ClientSideException("Sadece kapıda sayılmış (Approved) transferler raflanabilir.");

            var groupedPlacements = request.PlacedLines.GroupBy(p => p.DocumentLineId).ToList();
            foreach (var line in order.Lines)
            {
                var linePlacements = groupedPlacements.FirstOrDefault(g => g.Key == line.Id);
                var totalPlaced = linePlacements?.Sum(p => p.Quantity) ?? 0;

                if (totalPlaced != line.ReceivedQuantity)
                    throw new ClientSideException($"{line.Product?.Name} ürünü için kapıda {line.ReceivedQuantity} adet teslim alındı, ancak raflara {totalPlaced} adet atanıyor!");
            }

            await ApplyStockAndCapacityUpdates(request, warehouse, order.Id, nameof(TransferOrder), $"Transfer Putaway - Ref: {order.DocumentNumber}", MultiWarehouse.Entity.Enums.Inventory.MovementType.TransferIn, userId);

            order.Status = DocumentStatus.Completed;
            order.UpdatedDate = DateTime.UtcNow;
            _transferOrderRepository.Update(order);
        }

        private async Task ApplyStockAndCapacityUpdates(PutawayRequestDto request, Warehouse warehouse, Guid docId, string docType, string description, MultiWarehouse.Entity.Enums.Inventory.MovementType moveType, Guid userId)
        {
            var productIds = request.PlacedLines.Select(p => p.ProductId).Distinct().ToList();
            var shelfIds = request.PlacedLines.Select(p => p.ShelfId).Distinct().ToList();

            var productsDict = await _productRepository.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);
            var shelvesDict = await _shelfRepository.Where(s => shelfIds.Contains(s.Id)).Include(s => s.WarehouseZone).ToDictionaryAsync(s => s.Id);
            var existingStocksList = await _stockRepository.Where(s => s.WarehouseId == request.WarehouseId && productIds.Contains(s.ProductId) && s.IsActive).ToListAsync();

            double totalVolumeToAdd = 0;

            foreach (var line in request.PlacedLines)
            {
                if (!productsDict.TryGetValue(line.ProductId, out var product)) throw new ClientSideException("Ürün bulunamadı.");
                if (!shelvesDict.TryGetValue(line.ShelfId, out var shelf) || shelf.WarehouseZone?.WarehouseId != request.WarehouseId)
                    throw new ClientSideException($"Seçilen raf geçersiz veya bu depoya ait değil!");

                double unitVolume = product.Width * product.Height * product.Depth;
                double addedVolume = unitVolume * line.Quantity;
                double addedWeight = product.Weight * line.Quantity;
                totalVolumeToAdd += addedVolume;

                if (shelf.CurrentVolume + addedVolume > shelf.MaxVolume) throw new ClientSideException($"{product.Name} ürünü {shelf.ShelfNumber} rafına hacim olarak sığmıyor!");
                if (shelf.CurrentWeight + addedWeight > shelf.MaxWeight) throw new ClientSideException($"{product.Name} ürünü {shelf.ShelfNumber} rafı için çok ağır!");

                shelf.CurrentVolume += addedVolume;
                shelf.CurrentWeight += addedWeight;
                shelf.UpdatedDate = DateTime.UtcNow;
                shelf.Version = Guid.NewGuid();
                _shelfRepository.Update(shelf);

                var stock = existingStocksList.FirstOrDefault(s => s.ShelfId == line.ShelfId && s.ProductId == line.ProductId);
                if (stock != null)
                {
                    stock.Quantity += line.Quantity;
                    stock.UpdatedDate = DateTime.UtcNow;
                    stock.Version = Guid.NewGuid();
                    _stockRepository.Update(stock);
                }
                else
                {
                    var newStock = new Stock
                    {
                        WarehouseId = request.WarehouseId,
                        ShelfId = line.ShelfId,
                        ProductId = line.ProductId,
                        Quantity = line.Quantity,
                        ReservedQuantity = 0,
                        CreatedDate = DateTime.UtcNow,
                        IsActive = true,
                        Version = Guid.NewGuid()
                    };
                    await _stockRepository.AddAsync(newStock);
                    existingStocksList.Add(newStock);
                }

                await _stockMovementRepository.AddAsync(new StockMovement
                {
                    WarehouseId = request.WarehouseId,
                    ShelfId = line.ShelfId,
                    ProductId = line.ProductId,
                    MovementType = moveType,
                    Quantity = line.Quantity,
                    DocumentId = docId,
                    DocumentType = docType,
                    Description = description,
                    UserId = userId,
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                });
            }

            if (warehouse.UsedCapacity + totalVolumeToAdd > warehouse.MaxCapacity)
                throw new ClientSideException($"İşlem İptal Edildi: Hedef depo kapasitesi yetersiz!");

            warehouse.UsedCapacity += totalVolumeToAdd;
            warehouse.UpdatedDate = DateTime.UtcNow;
            _warehouseRepository.Update(warehouse);
        }

        #endregion

        #region Private Helpers
        private Guid GetCurrentUserId()
        {
            var userIdString = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdString, out var userId)) return userId;
            throw new UnauthorizedAccessException("Kullanıcı kimliği doğrulanamadı.");
        }

        private void ValidateRowLevelSecurity(Guid requestedWarehouseId)
        {
            var currentUserRole = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;
            if (currentUserRole != UserRole.SuperAdmin.ToString())
            {
                var claim = _httpContextAccessor.HttpContext?.User.FindFirst("WarehouseId");
                if (claim == null || !Guid.TryParse(claim.Value, out var currentWarehouseId))
                    throw new UnauthorizedAccessException("Kullanıcıya ait depo bilgisi bulunamadı.");

                if (currentWarehouseId != requestedWarehouseId)
                    throw new ClientSideException("Başka bir depoda işlem yapma yetkiniz bulunmamaktadır.");
            }
        }
        #endregion
    }
}