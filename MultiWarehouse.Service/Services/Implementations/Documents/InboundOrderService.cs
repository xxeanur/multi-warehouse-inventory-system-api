using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MultiWarehouse.Entity.Entities.Documents;
using MultiWarehouse.Entity.Entities.Identity;
using MultiWarehouse.Entity.Enums.Common;
using MultiWarehouse.Entity.Enums.Document;
using MultiWarehouse.Entity.Enums.User;
using MultiWarehouse.Service.Exceptions;
using MultiWarehouse.Service.Repositories.Interfaces;
using MultiWarehouse.Service.Services.Interfaces.Common;
using MultiWarehouse.Service.Services.Interfaces.Documents;
using MultiWarehouse.Service.Services.Interfaces.Notification;
using MultiWarehouse.Shared.DTOs.AuditLogDtos;
using MultiWarehouse.Shared.DTOs.DocumentDtos.InboundDtos;
using MultiWarehouse.Shared.DTOs.NotificationDtos;
using MultiWarehouse.Shared.Pagination;
using System.Security.Claims;

namespace MultiWarehouse.Service.Services.Implementations.Documents
{
    public class InboundOrderService : IInboundOrderService
    {
        #region Dependencies

        private readonly IGenericRepository<InboundOrder> _inboundRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly IAuditLogService _auditLogService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public InboundOrderService(
            IGenericRepository<InboundOrder> inboundRepository,
            IGenericRepository<User> userRepository,
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            IAuditLogService auditLogService,
            IHttpContextAccessor httpContextAccessor)
        {
            _inboundRepository = inboundRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _auditLogService = auditLogService;
            _httpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region Write Operations

        public async Task<Guid> CreateAsync(InboundOrderCreateDto createDto)
        {
            var currentUserRole = GetCurrentUserRole();
            if (currentUserRole == UserRole.Staff.ToString())
                throw new ClientSideException("Yetki İhlali: Saha personeli (Staff) yeni mal kabul fişi oluşturamaz!");

            ValidateRowLevelSecurity(createDto.WarehouseId);

            var duplicateCheck = createDto.Lines.GroupBy(l => l.ProductId).Any(g => g.Count() > 1);
            if (duplicateCheck)
                throw new ClientSideException("Bir ürün fiş üzerinde yalnızca tek bir satırda yer alabilir. Lütfen miktarları birleştirin.");

            if (createDto.MovementType == Entity.Enums.Inventory.MovementType.Inbound && createDto.SupplierId == null)
                throw new ClientSideException("Tedarikçiden mal kabul işlemlerinde 'Tedarikçi' seçimi zorunludur.");

            var documentNumber = await GenerateDocumentNumberAsync();
            var currentUserId = GetCurrentUserId();

            var order = new InboundOrder
            {
                DocumentNumber = documentNumber,
                SupplierId = createDto.SupplierId,
                WarehouseId = createDto.WarehouseId,
                MovementType = createDto.MovementType,
                Description = createDto.Description,
                Status = DocumentStatus.Pending,
                CreatedById = currentUserId
            };

            foreach (var lineDto in createDto.Lines)
            {
                order.Lines.Add(new InboundOrderLine
                {
                    ProductId = lineDto.ProductId,
                    ExpectedQuantity = lineDto.ExpectedQuantity,
                    ReceivedQuantity = 0
                });
            }

            await _inboundRepository.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            await LogActionAsync("InboundOrders", AuditActionType.DocumentCreated, $"Belge No: {order.DocumentNumber} oluşturuldu.");

            await NotifyWarehouseManagersAsync(order.WarehouseId, "Yeni Mal Kabul Fişi", $"{order.DocumentNumber} numaralı yeni bir mal kabul fişi oluşturuldu. Teslimat bekleniyor.", NotificationType.Inbound, NotificationTargetType.InboundOrder, order.Id);

            return order.Id;
        }

        public async Task ApproveAsync(InboundOrderApproveDto approveDto)
        {
            var order = await _inboundRepository.Where(o => o.Id == approveDto.InboundOrderId).Include(o => o.Lines).SingleOrDefaultAsync();
            if (order == null) throw new ClientSideException("Belge bulunamadı.");

            ValidateRowLevelSecurity(order.WarehouseId);

            if (order.Status != DocumentStatus.Pending)
                throw new ClientSideException("Sadece 'Beklemede' (Pending) olan belgeler sayılabilir ve kabul edilebilir.");

            foreach (var line in order.Lines)
            {
                var input = approveDto.ApprovedLines.FirstOrDefault(a => a.InboundOrderLineId == line.Id);
                int receivedQty = input?.ReceivedQuantity ?? 0;

                if (receivedQty != line.ExpectedQuantity)
                    throw new ClientSideException($"Kısmi teslimat kapalıdır! Satır ID {line.Id} için beklenen: {line.ExpectedQuantity}, kapıda sayılan: {receivedQty}");

                line.ReceivedQuantity = receivedQty;
            }

            order.Status = DocumentStatus.Approved;
            order.UpdatedDate = DateTime.UtcNow;
            order.ApprovedById = GetCurrentUserId();

            _inboundRepository.Update(order);
            await _unitOfWork.SaveChangesAsync();

            await LogActionAsync("InboundOrders", AuditActionType.DocumentApproved, $"Belge No: {order.DocumentNumber} kapıda sayılarak onaylandı.");
            await NotifyWarehouseManagersAsync(order.WarehouseId, "Mal Kabul Onaylandı", $"{order.DocumentNumber} numaralı mal kabul fişi sayılmış ve onaylanmıştır. Raflama (Putaway) işlemi beklenmektedir.", NotificationType.Inbound, NotificationTargetType.InboundOrder, order.Id);
        }

        public async Task CancelAsync(Guid inboundOrderId)
        {
            var currentUserRole = GetCurrentUserRole();
            if (currentUserRole == UserRole.Staff.ToString())
                throw new ClientSideException("Yetki İhlali: Saha personeli (Staff) mal kabul fişini iptal edemez!");

            var order = await _inboundRepository.Where(o => o.Id == inboundOrderId).SingleOrDefaultAsync();
            if (order == null) throw new ClientSideException("Belge bulunamadı.");

            ValidateRowLevelSecurity(order.WarehouseId);

            if (order.Status == DocumentStatus.Cancelled) throw new ClientSideException("Belge zaten iptal edilmiş.");
            if (order.Status == DocumentStatus.Completed) throw new ClientSideException("Tamamlanmış (Raflara işlenmiş) belgeler iptal edilemez. Lütfen iade veya transfer fişi oluşturun.");

            order.Status = DocumentStatus.Cancelled;
            order.UpdatedDate = DateTime.UtcNow;
            order.CancelledById = GetCurrentUserId();

            _inboundRepository.Update(order);
            await _unitOfWork.SaveChangesAsync();

            await LogActionAsync("InboundOrders", AuditActionType.DocumentCancelled, $"Belge No: {order.DocumentNumber} iptal edildi.");
            await NotifyWarehouseManagersAsync(order.WarehouseId, "Mal Kabul İptal Edildi", $"{order.DocumentNumber} numaralı mal kabul fişi iptal edilmiştir.", NotificationType.Inbound, NotificationTargetType.InboundOrder, order.Id);
        }

        #endregion

        #region Read Operations

        public async Task<IEnumerable<InboundOrderListDto>> GetAllAsync()
        {
            var query = GetBaseQueryWithRls();

            return await query
                .OrderByDescending(o => o.CreatedDate)
                .Select(o => new InboundOrderListDto
                {
                    Id = o.Id,
                    DocumentNumber = o.DocumentNumber,
                    SupplierName = o.Supplier != null ? o.Supplier.CompanyName : "-",
                    WarehouseId = o.WarehouseId,
                    WarehouseName = o.Warehouse != null ? o.Warehouse.Name : "-",
                    MovementType = o.MovementType,
                    Status = o.Status,
                    CreatedDate = o.CreatedDate,
                    SourceTransferOrderId = o.SourceTransferOrderId
                }).ToListAsync();
        }

        public async Task<PagedResult<InboundOrderListDto>> GetPagedAsync(PaginationParams paginationParams)
        {
            var query = GetBaseQueryWithRls();
            var totalCount = await query.CountAsync();

            var data = await query
                .OrderByDescending(o => o.CreatedDate)
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .Select(o => new InboundOrderListDto
                {
                    Id = o.Id,
                    DocumentNumber = o.DocumentNumber,
                    SupplierName = o.Supplier != null ? o.Supplier.CompanyName : "-",
                    WarehouseId = o.WarehouseId,
                    WarehouseName = o.Warehouse != null ? o.Warehouse.Name : "-",
                    MovementType = o.MovementType,
                    Status = o.Status,
                    CreatedDate = o.CreatedDate,
                    SourceTransferOrderId = o.SourceTransferOrderId
                }).ToListAsync();

            return new PagedResult<InboundOrderListDto>(data, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
        }

        public async Task<IEnumerable<InboundOrderListDto>> GetAllByWarehouseIdAsync(Guid warehouseId)
        {
            ValidateRowLevelSecurity(warehouseId);

            var query = _inboundRepository.Where(o => o.WarehouseId == warehouseId);

            return await query
                .OrderByDescending(o => o.CreatedDate)
                .Select(o => new InboundOrderListDto
                {
                    Id = o.Id,
                    DocumentNumber = o.DocumentNumber,
                    SupplierName = o.Supplier != null ? o.Supplier.CompanyName : "-",
                    WarehouseId = o.WarehouseId,
                    WarehouseName = o.Warehouse != null ? o.Warehouse.Name : "-",
                    MovementType = o.MovementType,
                    Status = o.Status,
                    CreatedDate = o.CreatedDate,
                    SourceTransferOrderId = o.SourceTransferOrderId
                }).ToListAsync();
        }

        public async Task<InboundOrderDetailDto> GetByIdAsync(Guid id)
        {
            var order = await _inboundRepository
                .Where(o => o.Id == id)
                .Select(o => new InboundOrderDetailDto
                {
                    Id = o.Id,
                    DocumentNumber = o.DocumentNumber,
                    SupplierName = o.Supplier != null ? o.Supplier.CompanyName : "-",
                    WarehouseId = o.WarehouseId,
                    WarehouseName = o.Warehouse != null ? o.Warehouse.Name : "-",
                    MovementType = o.MovementType,
                    Status = o.Status,
                    CreatedDate = o.CreatedDate,
                    SourceTransferOrderId = o.SourceTransferOrderId,
                    Description = o.Description,

                    CreatedByName = o.CreatedBy != null ? o.CreatedBy.FirstName + " " + o.CreatedBy.LastName : "-",
                    ApprovedByName = o.ApprovedBy != null ? o.ApprovedBy.FirstName + " " + o.ApprovedBy.LastName : null,
                    CancelledByName = o.CancelledBy != null ? o.CancelledBy.FirstName + " " + o.CancelledBy.LastName : null,

                    Lines = o.Lines.Select(l => new InboundOrderLineDto
                    {
                        Id = l.Id,
                        ProductId = l.ProductId,
                        ProductName = l.Product != null ? l.Product.Name : "-",
                        ProductCode = l.Product != null ? l.Product.Sku : "-",
                        ExpectedQuantity = l.ExpectedQuantity,
                        ReceivedQuantity = l.ReceivedQuantity
                    }).ToList()
                })
                .SingleOrDefaultAsync();

            if (order == null) throw new ClientSideException("Belge bulunamadı.");

            ValidateRowLevelSecurity(order.WarehouseId);

            return order;
        }

        #endregion

        #region Private Helpers

        private IQueryable<InboundOrder> GetBaseQueryWithRls()
        {
            var query = _inboundRepository.GetAll();
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
                    throw new ClientSideException("Başka bir deponun fişleri üzerinde işlem yapma veya görüntüleme yetkiniz bulunmamaktadır.");
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

        private Guid? GetCurrentUserId()
        {
            var userIdStr = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdStr, out var userId)) return userId;
            return null;
        }

        private async Task<string> GenerateDocumentNumberAsync()
        {
            var today = DateTime.UtcNow.Date;

            var lastOrder = await _inboundRepository
                .Where(o => o.CreatedDate >= today)
                .OrderByDescending(o => o.DocumentNumber)
                .FirstOrDefaultAsync();

            if (lastOrder == null) return $"INB-{today:yyyyMMdd}-000001";

            var lastNumberStr = lastOrder.DocumentNumber.Substring(lastOrder.DocumentNumber.Length - 6);

            if (int.TryParse(lastNumberStr, out int lastNumber)) return $"INB-{today:yyyyMMdd}-{(lastNumber + 1):D6}";

            var count = await _inboundRepository.Where(o => o.CreatedDate >= today).CountAsync();
            return $"INB-{today:yyyyMMdd}-{(count + 1):D6}";
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

        private async Task LogActionAsync(string tableName, AuditActionType actionType, string details)
        {
            var userId = GetCurrentUserId();
            var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Bilinmiyor";

            if (userId.HasValue)
            {
                await _auditLogService.CreateAsync(new AuditLogCreateDto
                {
                    UserId = userId.Value,
                    ActionType = actionType,
                    TableName = tableName,
                    OldValues = string.Empty,
                    NewValues = details,
                    IpAddress = ipAddress
                });
            }
        }

        #endregion
    }
}