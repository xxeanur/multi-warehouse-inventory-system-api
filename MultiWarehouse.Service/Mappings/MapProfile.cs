using AutoMapper;
using MultiWarehouse.Entity.Entities.Common;
using MultiWarehouse.Entity.Entities.Definitions;
using MultiWarehouse.Entity.Entities.Documents;
using MultiWarehouse.Entity.Entities.Identity;
using MultiWarehouse.Entity.Entities.Inventory;
using MultiWarehouse.Entity.Entities.Notification;
using MultiWarehouse.Shared.DTOs.AuditLogDtos;
using MultiWarehouse.Shared.DTOs.CategoryDtos;
using MultiWarehouse.Shared.DTOs.DocumentDtos.InboundDtos;
using MultiWarehouse.Shared.DTOs.DocumentDtos.OutboundDtos;
using MultiWarehouse.Shared.DTOs.DocumentDtos.TransferDtos;
using MultiWarehouse.Shared.DTOs.InventoryDtos;
using MultiWarehouse.Shared.DTOs.NotificationDtos;
using MultiWarehouse.Shared.DTOs.ProductDtos;
using MultiWarehouse.Shared.DTOs.ShelfDtos;
using MultiWarehouse.Shared.DTOs.StockDtos;
using MultiWarehouse.Shared.DTOs.StockMovementDtos;
using MultiWarehouse.Shared.DTOs.SupplierDtos;
using MultiWarehouse.Shared.DTOs.UserDtos;
using MultiWarehouse.Shared.DTOs.WarehouseDtos;
using MultiWarehouse.Shared.DTOs.WarehouseZoneDtos;
using MultiWarehouse.Shared.Pagination;

namespace MultiWarehouse.Service.Mappings
{
    public class MapProfile : Profile
    {
        public MapProfile()
        {
            // --- User Mappings ---
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<UserCreateDto, User>();

            // --- Category Mappings ---
            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<CategoryCreateDto, Category>();
            CreateMap<CategoryUpdateDto, Category>();

            // Supplier Mappings
            CreateMap<Supplier, SupplierDto>().ReverseMap();
            CreateMap<SupplierCreateDto, Supplier>();
            CreateMap<SupplierUpdateDto, Supplier>();

            // Warehouse Mappings
            CreateMap<Warehouse, WarehouseDto>().ReverseMap();
            CreateMap<WarehouseCreateDto, Warehouse>();
            CreateMap<WarehouseUpdateDto, Warehouse>();

            // WarehouseZone Mappings
            CreateMap<WarehouseZone, WarehouseZoneDto>().ReverseMap();
            CreateMap<WarehouseZoneCreateDto, WarehouseZone>();
            CreateMap<WarehouseZoneUpdateDto, WarehouseZone>();

            // Shelf Mappings
            CreateMap<Shelf, ShelfDto>().ReverseMap();
            CreateMap<ShelfCreateDto, Shelf>();
            CreateMap<ShelfUpdateDto, Shelf>();

            // Product Mappings
            CreateMap<Product, ProductDto>().ReverseMap();
            CreateMap<ProductCreateDto, Product>();
            CreateMap<ProductUpdateDto, Product>();

            // Stock Mappings
            CreateMap<Stock, StockDto>().ReverseMap();

            // StockMovement Mappings
            CreateMap<StockMovement, StockMovementListDto>()
                .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
                .ForMember(dest => dest.ShelfCode, opt => opt.MapFrom(src => src.Shelf != null ? src.Shelf.ShelfNumber : string.Empty))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
                .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Product != null ? src.Product.Sku : string.Empty));

            CreateMap<StockMovement, StockMovementDetailDto>()
                .IncludeBase<StockMovement, StockMovementListDto>();

            // ==========================================
            // DOCUMENT (BELGE) MAPPINGS (INBOUND, OUTBOUND, TRANSFER)
            // ==========================================

            // --- 1. INBOUND MAPPINGS ---
            CreateMap<InboundOrder, InboundOrderListDto>()
                .ForMember(dest => dest.WarehouseId, opt => opt.MapFrom(src => src.WarehouseId))
                .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.CompanyName : string.Empty));

            CreateMap<InboundOrder, InboundOrderDetailDto>()
                .IncludeBase<InboundOrder, InboundOrderListDto>();

            CreateMap<InboundOrderLine, InboundOrderLineDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
                .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Product != null ? src.Product.Sku : string.Empty));

            // --- 2. OUTBOUND MAPPINGS ---
            CreateMap<OutboundOrder, OutboundOrderListDto>()
                .ForMember(dest => dest.WarehouseId, opt => opt.MapFrom(src => src.WarehouseId))
                .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty));

            CreateMap<OutboundOrder, OutboundOrderDetailDto>()
                .IncludeBase<OutboundOrder, OutboundOrderListDto>();

            CreateMap<OutboundOrderLine, OutboundOrderLineDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
                .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Product != null ? src.Product.Sku : string.Empty));

            // --- 3. TRANSFER MAPPINGS ---
            CreateMap<TransferOrder, TransferOrderListDto>()
                .ForMember(dest => dest.SourceWarehouseId, opt => opt.MapFrom(src => src.SourceWarehouseId))
                .ForMember(dest => dest.TargetWarehouseId, opt => opt.MapFrom(src => src.TargetWarehouseId))
                .ForMember(dest => dest.SourceWarehouseName, opt => opt.MapFrom(src => src.SourceWarehouse != null ? src.SourceWarehouse.Name : string.Empty))
                .ForMember(dest => dest.TargetWarehouseName, opt => opt.MapFrom(src => src.TargetWarehouse != null ? src.TargetWarehouse.Name : string.Empty));

            CreateMap<TransferOrder, TransferOrderDetailDto>()
                .IncludeBase<TransferOrder, TransferOrderListDto>();

            CreateMap<TransferOrderLine, TransferOrderLineDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
                .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Product != null ? src.Product.Sku : string.Empty));

            // Notification Mappings
            CreateMap<Notification, NotificationDto>().ReverseMap();
            CreateMap<NotificationCreateDto, Notification>();

            // AuditLog Mappings
            CreateMap<AuditLog, AuditLogDto>().ReverseMap();

            CreateMap<AuditLogCreateDto, AuditLog>();

            CreateMap(typeof(PagedResult<>), typeof(PagedResult<>));
        }
    }
}