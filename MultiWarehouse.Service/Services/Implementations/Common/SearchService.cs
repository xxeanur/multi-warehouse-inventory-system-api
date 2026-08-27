using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MultiWarehouse.Entity.Entities.Definitions;
using MultiWarehouse.Entity.Entities.Documents;
using MultiWarehouse.Entity.Enums.Common;
using MultiWarehouse.Entity.Enums.User;
using MultiWarehouse.Service.Repositories.Interfaces;
using MultiWarehouse.Service.Services.Interfaces.Common;
using MultiWarehouse.Shared.DTOs.SearchDtos;
using System.Security.Claims;

namespace MultiWarehouse.Service.Services.Implementations.Common
{
    public class SearchService : ISearchService
    {
        #region Dependencies

        private readonly IGenericRepository<Product> _productRepository;
        private readonly IGenericRepository<InboundOrder> _inboundRepository;
        private readonly IGenericRepository<OutboundOrder> _outboundRepository;
        private readonly IGenericRepository<TransferOrder> _transferRepository;
        private readonly IGenericRepository<Warehouse> _warehouseRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SearchService(
            IGenericRepository<Product> productRepository,
            IGenericRepository<InboundOrder> inboundRepository,
            IGenericRepository<OutboundOrder> outboundRepository,
            IGenericRepository<TransferOrder> transferRepository,
            IGenericRepository<Warehouse> warehouseRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _productRepository = productRepository;
            _inboundRepository = inboundRepository;
            _outboundRepository = outboundRepository;
            _transferRepository = transferRepository;
            _warehouseRepository = warehouseRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region Read Operations

        public async Task<IEnumerable<SearchResultItemDto>> SearchAcrossModulesAsync(string query)
        {
            var results = new List<SearchResultItemDto>();
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return results;

            string lowerQuery = query.ToLower();

            var currentUserRole = GetCurrentUserRole();
            var currentWarehouseId = GetCurrentWarehouseId();
            bool isSuperAdmin = currentUserRole == UserRole.SuperAdmin.ToString();
            bool isStaff = currentUserRole == UserRole.Staff.ToString();

            var products = await _productRepository
                .Where(p => p.IsActive && (p.Name.ToLower().Contains(lowerQuery) || p.Sku.ToLower().Contains(lowerQuery)))
                .OrderBy(p => p.Name)
                .Take(4)
                .Select(p => new SearchResultItemDto
                {
                    Category = "Ürünler",
                    Title = p.Name,
                    Subtitle = $"SKU: {p.Sku}",
                    TargetType = SearchTargetType.Product,
                    TargetId = p.Id
                }).ToListAsync();
            results.AddRange(products);

            var inbounds = await _inboundRepository
                .Where(i => i.DocumentNumber.ToLower().Contains(lowerQuery) &&
                            (isSuperAdmin || i.WarehouseId == currentWarehouseId))
                .OrderByDescending(i => i.CreatedDate)
                .Take(3)
                .Select(i => new SearchResultItemDto
                {
                    Category = "Mal Kabul (Inbound)",
                    Title = i.DocumentNumber,
                    Subtitle = "Fiş Detayı",
                    TargetType = SearchTargetType.InboundOrder,
                    TargetId = i.Id
                }).ToListAsync();
            results.AddRange(inbounds);

            var outbounds = await _outboundRepository
                .Where(o => o.DocumentNumber.ToLower().Contains(lowerQuery) &&
                            (isSuperAdmin || o.WarehouseId == currentWarehouseId))
                .OrderByDescending(o => o.CreatedDate)
                .Take(3)
                .Select(o => new SearchResultItemDto
                {
                    Category = "Sevkiyat (Outbound)",
                    Title = o.DocumentNumber,
                    Subtitle = "Fiş Detayı",
                    TargetType = SearchTargetType.OutboundOrder,
                    TargetId = o.Id
                }).ToListAsync();
            results.AddRange(outbounds);

            var transfers = await _transferRepository
                .Where(t => t.DocumentNumber.ToLower().Contains(lowerQuery) &&
                            (isSuperAdmin || t.SourceWarehouseId == currentWarehouseId || t.TargetWarehouseId == currentWarehouseId))
                .OrderByDescending(t => t.CreatedDate)
                .Take(3)
                .Select(t => new SearchResultItemDto
                {
                    Category = "Transfer (Transfer)",
                    Title = t.DocumentNumber,
                    Subtitle = "Fiş Detayı",
                    TargetType = SearchTargetType.TransferOrder,
                    TargetId = t.Id
                }).ToListAsync();
            results.AddRange(transfers);

            var warehouses = await _warehouseRepository
                .Where(w => w.IsActive && w.Name.ToLower().Contains(lowerQuery) &&
                            (isSuperAdmin || w.Id == currentWarehouseId))
                .OrderBy(w => w.Name)
                .Take(2)
                .Select(w => new SearchResultItemDto
                {
                    Category = "Depolar",
                    Title = w.Name,
                    Subtitle = "Depo Yönetimi",
                    TargetType = SearchTargetType.Warehouse,
                    TargetId = w.Id
                }).ToListAsync();
            results.AddRange(warehouses);


            return results;
        }

        #endregion

        #region Private Helpers

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