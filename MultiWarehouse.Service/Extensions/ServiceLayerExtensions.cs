using Microsoft.Extensions.DependencyInjection;
using MultiWarehouse.Service.Services.Implementations;
using MultiWarehouse.Service.Services.Implementations.Common;
using MultiWarehouse.Service.Services.Implementations.Definitions;
using MultiWarehouse.Service.Services.Implementations.Documents;
using MultiWarehouse.Service.Services.Implementations.Identity;
using MultiWarehouse.Service.Services.Implementations.Inventory;
using MultiWarehouse.Service.Services.Implementations.Notification;
using MultiWarehouse.Service.Services.Interfaces.Common;
using MultiWarehouse.Service.Services.Interfaces.Dashboard;
using MultiWarehouse.Service.Services.Interfaces.Definations;
using MultiWarehouse.Service.Services.Interfaces.Documents;
using MultiWarehouse.Service.Services.Interfaces.Identity;
using MultiWarehouse.Service.Services.Interfaces.Inventory;
using MultiWarehouse.Service.Services.Interfaces.Notification;
using System.Reflection;

namespace MultiWarehouse.Service.Extensions
{
    public static class ServiceLayerExtensions
    {
        public static IServiceCollection AddServiceLayer(this IServiceCollection services)
        {
            // 1. AutoMapper Kaydı (Bu katmandaki tüm Profile sınıflarını otomatik bulur)
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            // 2. Identity / Auth Servisleri
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();

            // 3. Definition (Tanım) Servisleri
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ISupplierService, SupplierService>();
            services.AddScoped<IWarehouseService, WarehouseService>();
            services.AddScoped<IWarehouseZoneService, WarehouseZoneService>();
            services.AddScoped<IProductService, ProductService>();

            // 4. Inventory (Envanter) Servisleri
            services.AddScoped<IShelfService, ShelfService>();
            services.AddScoped<IStockService, StockService>();
            services.AddScoped<IStockMovementService, StockMovementService>();

            // 5. Document (Belge) Servisleri
            services.AddScoped<IInboundOrderService, InboundOrderService>();
            services.AddScoped<IOutboundOrderService, OutboundOrderService>();
            services.AddScoped<ITransferOrderService, TransferOrderService>();

            // 6. Notification (Bildirim) Servisleri
            services.AddScoped<INotificationService, NotificationService>();

            // 7. Log Servisleri
            services.AddScoped<IAuditLogService, AuditLogService>();

            // 8. Dashboard Servisleri (Yorumu kaldırdım)
            services.AddScoped<IDashboardService, DashboardService>();

            //9. rafa yerlleştirme
            services.AddScoped<IPutawayService, PutawayService>();

            //10. count işlemleri
            services.AddScoped<IInventoryCountService, InventoryCountService>();

            //11.mail
            services.AddScoped<IEmailService, EmailService>();

            //12.search
            services.AddScoped<ISearchService, SearchService>();

            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}