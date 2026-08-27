using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MultiWarehouse.Entity.Entities.Documents;
using MultiWarehouse.Entity.Entities.Inventory;
using MultiWarehouse.Entity.Enums.Inventory;
using MultiWarehouse.Entity.Enums.User;
using MultiWarehouse.Service.Exceptions;
using MultiWarehouse.Service.Repositories.Interfaces;
using MultiWarehouse.Service.Services.Interfaces.Inventory;
using MultiWarehouse.Shared.DTOs.InventoryDtos;
using MultiWarehouse.Shared.DTOs.StockMovementDtos;
using MultiWarehouse.Shared.Pagination;
using System.Security.Claims;

namespace MultiWarehouse.Service.Services.Implementations.Inventory
{
    public class StockMovementService : IStockMovementService
    {
        #region Dependencies

        private readonly IGenericRepository<StockMovement> _movementRepository;
        private readonly IGenericRepository<InboundOrder> _inboundRepository;
        private readonly IGenericRepository<OutboundOrder> _outboundRepository;
        private readonly IGenericRepository<TransferOrder> _transferRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public StockMovementService(
            IGenericRepository<StockMovement> movementRepository,
            IGenericRepository<InboundOrder> inboundRepository,
            IGenericRepository<OutboundOrder> outboundRepository,
            IGenericRepository<TransferOrder> transferRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _movementRepository = movementRepository;
            _inboundRepository = inboundRepository;
            _outboundRepository = outboundRepository;
            _transferRepository = transferRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region Read Operations (Paged & Row-Level Security)

        public async Task<PagedResult<StockMovementListDto>> GetFilteredPagedAsync(StockMovementFilterDto filterDto, PaginationParams paginationParams)
        {
            var query = _movementRepository.GetAll();
            var currentUserRole = GetCurrentUserRole();

            if (currentUserRole != UserRole.SuperAdmin.ToString())
            {
                var currentWarehouseId = GetCurrentWarehouseId();
                if (currentWarehouseId == null) throw new UnauthorizedAccessException("Kullanıcı depo yetkisi bulunamadı.");

                query = query.Where(sm => sm.WarehouseId == currentWarehouseId);
            }

            if (!string.IsNullOrWhiteSpace(filterDto.Direction))
            {
                var targetTypes = MovementTypeExtensions.GetTypesByDirection(filterDto.Direction);
                if (targetTypes.Any())
                {
                    query = query.Where(sm => targetTypes.Contains(sm.MovementType));
                }
            }

            if (filterDto.WarehouseId.HasValue) query = query.Where(sm => sm.WarehouseId == filterDto.WarehouseId.Value);
            if (filterDto.ShelfId.HasValue) query = query.Where(sm => sm.ShelfId == filterDto.ShelfId.Value);
            if (filterDto.ProductId.HasValue) query = query.Where(sm => sm.ProductId == filterDto.ProductId.Value);
            if (filterDto.MovementType.HasValue) query = query.Where(sm => sm.MovementType == filterDto.MovementType.Value);
            if (filterDto.DocumentId.HasValue) query = query.Where(sm => sm.DocumentId == filterDto.DocumentId.Value);
            if (filterDto.StartDate.HasValue) query = query.Where(sm => sm.CreatedDate >= filterDto.StartDate.Value);
            if (filterDto.EndDate.HasValue) query = query.Where(sm => sm.CreatedDate <= filterDto.EndDate.Value);

            if (!string.IsNullOrWhiteSpace(filterDto.SearchTerm))
            {
                var search = filterDto.SearchTerm.ToLower();
                query = query.Where(sm =>
                    (sm.Product != null && sm.Product.Name.ToLower().Contains(search)) ||
                    (sm.Product != null && sm.Product.Sku.ToLower().Contains(search)) ||
                    (sm.Shelf != null && sm.Shelf.ShelfNumber.ToLower().Contains(search)) ||
                    sm.Description.ToLower().Contains(search)
                );
            }

            var totalCount = await query.CountAsync();

            var projectedQuery = query
                .OrderByDescending(sm => sm.CreatedDate)
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .Select(sm => new
                {
                    sm.Id,
                    sm.WarehouseId,
                    WarehouseName = sm.Warehouse != null ? sm.Warehouse.Name : "Bilinmeyen Depo",
                    sm.ShelfId,
                    ShelfCode = sm.Shelf != null ? sm.Shelf.ShelfNumber : "Belirsiz",
                    sm.ProductId,
                    ProductName = sm.Product != null ? sm.Product.Name : "Silinmiş Ürün",
                    ProductCode = sm.Product != null ? sm.Product.Sku : "-",
                    sm.MovementType,
                    sm.Quantity,
                    sm.DocumentId,
                    sm.DocumentType,
                    sm.Description,
                    sm.UserId,
                    OperatorFirstName = sm.User != null ? sm.User.FirstName : "Bilinmeyen",
                    OperatorLastName = sm.User != null ? sm.User.LastName : "Operatör",
                    sm.CreatedDate
                });

            var pagedData = await projectedQuery.ToListAsync();

            var mappedItems = pagedData.Select(sm => new StockMovementListDto
            {
                Id = sm.Id,
                WarehouseId = sm.WarehouseId,
                WarehouseName = sm.WarehouseName,
                ShelfId = sm.ShelfId,
                ShelfCode = sm.ShelfCode,
                ProductId = sm.ProductId,
                ProductName = sm.ProductName,
                ProductCode = sm.ProductCode,
                MovementType = sm.MovementType,
                MovementDirection = sm.MovementType.GetDirectionName(),
                MovementTypeName = sm.MovementType.GetTypeName(),
                Quantity = sm.Quantity,
                DocumentId = sm.DocumentId,
                DocumentType = sm.DocumentType,
                Description = sm.Description,
                UserId = sm.UserId,
                OperatorName = $"{sm.OperatorFirstName} {sm.OperatorLastName}",
                CreatedDate = sm.CreatedDate
            }).ToList();

            return new PagedResult<StockMovementListDto>(mappedItems, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
        }

        public async Task<StockMovementDetailDto> GetDetailByIdAsync(Guid id)
        {
            var movement = await _movementRepository.Where(sm => sm.Id == id).Select(sm => new
            {
                sm.Id,
                sm.WarehouseId,
                WarehouseName = sm.Warehouse != null ? sm.Warehouse.Name : "Bilinmeyen Depo",
                sm.ShelfId,
                ShelfCode = sm.Shelf != null ? sm.Shelf.ShelfNumber : "Belirsiz",
                sm.ProductId,
                ProductName = sm.Product != null ? sm.Product.Name : "Silinmiş Ürün",
                ProductCode = sm.Product != null ? sm.Product.Sku : "-",
                sm.MovementType,
                sm.Quantity,
                sm.DocumentId,
                sm.DocumentType,
                sm.Description,
                sm.UserId,
                OperatorFirstName = sm.User != null ? sm.User.FirstName : "Bilinmeyen",
                OperatorLastName = sm.User != null ? sm.User.LastName : "Operatör",
                OperatorEmail = sm.User != null ? sm.User.Email : "-",
                OperatorRole = sm.User != null ? sm.User.Role.ToString() : "-",
                sm.IsCancelled,
                sm.CreatedDate
            }).FirstOrDefaultAsync();

            if (movement == null)
                throw new ClientSideException("Stok hareketi bulunamadı.");

            var currentUserRole = GetCurrentUserRole();
            if (currentUserRole != UserRole.SuperAdmin.ToString())
            {
                var currentWarehouseId = GetCurrentWarehouseId();
                if (currentWarehouseId != movement.WarehouseId)
                    throw new ClientSideException("Başka bir depoya ait hareketi görüntüleme yetkiniz yok.");
            }

            string documentReference = "Manuel Lokal İşlem";

            if (movement.DocumentId != Guid.Empty && !string.IsNullOrWhiteSpace(movement.DocumentType))
            {
                if (movement.DocumentType == "InboundOrder")
                {
                    var doc = await _inboundRepository.Where(d => d.Id == movement.DocumentId).FirstOrDefaultAsync();
                    if (doc != null) documentReference = $"Mal Kabul Fişi: {doc.DocumentNumber}";
                }
                else if (movement.DocumentType == "OutboundOrder")
                {
                    var doc = await _outboundRepository.Where(d => d.Id == movement.DocumentId).FirstOrDefaultAsync();
                    if (doc != null) documentReference = $"Sevkiyat Fişi: {doc.DocumentNumber}";
                }
                else if (movement.DocumentType == "TransferOrder")
                {
                    var doc = await _transferRepository.Where(d => d.Id == movement.DocumentId).FirstOrDefaultAsync();
                    if (doc != null) documentReference = $"Transfer Fişi: {doc.DocumentNumber}";
                }
                else
                {
                    documentReference = $"{movement.DocumentType} - {movement.DocumentId.ToString().Substring(0, 8).ToUpper()}";
                }
            }

            return new StockMovementDetailDto
            {
                Id = movement.Id,
                WarehouseId = movement.WarehouseId,
                WarehouseName = movement.WarehouseName,
                ShelfId = movement.ShelfId,
                ShelfCode = movement.ShelfCode,
                ProductId = movement.ProductId,
                ProductName = movement.ProductName,
                ProductCode = movement.ProductCode,
                MovementType = movement.MovementType,
                MovementDirection = movement.MovementType.GetDirectionName(),
                MovementTypeName = movement.MovementType.GetTypeName(),
                Quantity = movement.Quantity,
                DocumentId = movement.DocumentId,
                DocumentType = movement.DocumentType,
                Description = movement.Description,
                UserId = movement.UserId,
                OperatorName = $"{movement.OperatorFirstName} {movement.OperatorLastName}",
                OperatorEmail = movement.OperatorEmail,
                OperatorRole = movement.OperatorRole,
                DocumentReference = documentReference,
                IsCancelled = movement.IsCancelled,
                CreatedDate = movement.CreatedDate
            };
        }

        public async Task<PagedResult<StockMovementListDto>> GetByProductIdAsync(Guid productId, PaginationParams paginationParams)
        {
            return await GetMovementsByConditionPagedAsync(sm => sm.ProductId == productId, paginationParams);
        }

        public async Task<PagedResult<StockMovementListDto>> GetByShelfIdAsync(Guid shelfId, PaginationParams paginationParams)
        {
            return await GetMovementsByConditionPagedAsync(sm => sm.ShelfId == shelfId, paginationParams);
        }

        public async Task<PagedResult<StockMovementListDto>> GetByDocumentIdAsync(Guid documentId, PaginationParams paginationParams)
        {
            return await GetMovementsByConditionPagedAsync(sm => sm.DocumentId == documentId, paginationParams);
        }

        #endregion

        #region Private Helpers

        private async Task<PagedResult<StockMovementListDto>> GetMovementsByConditionPagedAsync(
            System.Linq.Expressions.Expression<Func<StockMovement, bool>> condition,
            PaginationParams paginationParams)
        {
            var query = _movementRepository.Where(condition);
            var currentUserRole = GetCurrentUserRole();

            if (currentUserRole != UserRole.SuperAdmin.ToString())
            {
                var currentWarehouseId = GetCurrentWarehouseId();
                query = query.Where(sm => sm.WarehouseId == currentWarehouseId);
            }

            var totalCount = await query.CountAsync();

            var rawData = await query
                .OrderByDescending(sm => sm.CreatedDate)
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .Select(sm => new
                {
                    sm.Id,
                    sm.WarehouseId,
                    WarehouseName = sm.Warehouse != null ? sm.Warehouse.Name : "Bilinmeyen Depo",
                    sm.ShelfId,
                    ShelfCode = sm.Shelf != null ? sm.Shelf.ShelfNumber : "Belirsiz",
                    sm.ProductId,
                    ProductName = sm.Product != null ? sm.Product.Name : "Silinmiş Ürün",
                    ProductCode = sm.Product != null ? sm.Product.Sku : "-",
                    sm.MovementType,
                    sm.Quantity,
                    sm.DocumentId,
                    sm.DocumentType,
                    sm.Description,
                    sm.UserId,
                    OperatorFirstName = sm.User != null ? sm.User.FirstName : "Bilinmeyen",
                    OperatorLastName = sm.User != null ? sm.User.LastName : "Operatör",
                    sm.CreatedDate
                }).ToListAsync();

            var mappedList = rawData.Select(sm => new StockMovementListDto
            {
                Id = sm.Id,
                WarehouseId = sm.WarehouseId,
                WarehouseName = sm.WarehouseName,
                ShelfId = sm.ShelfId,
                ShelfCode = sm.ShelfCode,
                ProductId = sm.ProductId,
                ProductName = sm.ProductName,
                ProductCode = sm.ProductCode,
                MovementType = sm.MovementType,
                MovementDirection = sm.MovementType.GetDirectionName(),
                MovementTypeName = sm.MovementType.GetTypeName(),
                Quantity = sm.Quantity,
                DocumentId = sm.DocumentId,
                DocumentType = sm.DocumentType,
                Description = sm.Description,
                UserId = sm.UserId,
                OperatorName = $"{sm.OperatorFirstName} {sm.OperatorLastName}",
                CreatedDate = sm.CreatedDate
            }).ToList();

            return new PagedResult<StockMovementListDto>(mappedList, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
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

        #endregion
    }
}