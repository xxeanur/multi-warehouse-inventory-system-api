using AutoMapper;
using MultiWarehouse.Entity.Entities;
using MultiWarehouse.Shared.DTOs.AuditLogDtos;
using MultiWarehouse.Shared.DTOs.CategoryDtos;
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
            CreateMap<StockCreateDto, Stock>();
            CreateMap<StockUpdateDto, Stock>();

            // StockMovement Mappings
            CreateMap<StockMovement, StockMovementDto>().ReverseMap();
            CreateMap<StockMovementCreateDto, StockMovement>();
            CreateMap<StockMovementUpdateDto, StockMovement>();

            // Notification Mappings
            CreateMap<Notification, NotificationDto>().ReverseMap();
            CreateMap<NotificationCreateDto, Notification>();

            // AuditLog Mappings
            CreateMap<AuditLog, AuditLogDto>().ReverseMap();
            CreateMap<AuditLogCreateDto, AuditLog>();

            // PagedResult<Entity> -> PagedResult<Dto> dönüşümünü otomatik tanıması için
            CreateMap(typeof(PagedResult<>), typeof(PagedResult<>));
        }
    }
}