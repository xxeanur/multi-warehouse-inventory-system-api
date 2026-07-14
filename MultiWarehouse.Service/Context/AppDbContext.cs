using Microsoft.EntityFrameworkCore;
using MultiWarehouse.Entity.Entities;
using MultiWarehouse.Entity.Enums;

namespace MultiWarehouse.Service.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Tüm Tablolar (SystemSettings çıkarıldı, Warehouse eklendi)
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<WarehouseZone> WarehouseZones { get; set; }
        public DbSet<Shelf> Shelves { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<InventoryCount> InventoryCounts { get; set; }
        public DbSet<InventoryCountDetail> InventoryCountDetails { get; set; }

        // Auth & Güvenlik
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // İlişkiler (Foreign Keys)

            // 1. İlişki: 1 WarehouseZone (Blok) -> N Shelf (Raf)
            modelBuilder.Entity<Shelf>()
                .HasOne(s => s.WarehouseZone)
                .WithMany(wz => wz.Shelves) // w harfini kafa karıştırmaması için wz (WarehouseZone) yaptık
                .HasForeignKey(s => s.WarehouseZoneId)
                .OnDelete(DeleteBehavior.Cascade);

            // 2. İlişki: 1 Warehouse (Depo) -> N WarehouseZone (Blok)
            modelBuilder.Entity<WarehouseZone>()
                .HasOne(wz => wz.Warehouse)
                .WithMany(w => w.WarehouseZones) // Burada w (Warehouse) temsil ediyor
                .HasForeignKey(wz => wz.WarehouseId)
                .OnDelete(DeleteBehavior.Cascade);

            // 3. İlişki: Ürün ve Kategori
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // 4. İlişki: Ürün ve Tedarikçi
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            // İlişki: 1 Product (Ürün) -> N Stock (Farklı raflardaki stok kayıtları)
            modelBuilder.Entity<Stock>()
                .HasOne(s => s.Product)
                .WithMany(p => p.Stocks)
                .HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.Cascade); // Ürün silinirse, o ürüne ait stok kayıtları da silinsin

            // İlişki: 1 InventoryCount -> N InventoryCountDetail
            modelBuilder.Entity<InventoryCountDetail>()
                .HasOne(d => d.InventoryCount)
                .WithMany(c => c.CountDetails)
                .HasForeignKey(d => d.InventoryCountId)
                .OnDelete(DeleteBehavior.Cascade); // Ana sayım silinirse, detayları (satırları) da silinsin

            // İlişki: 1 User -> N RefreshToken
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // İlişki: 1 User -> N PasswordResetToken
            modelBuilder.Entity<PasswordResetToken>()
                .HasOne(prt => prt.User)
                .WithMany(u => u.PasswordResetTokens)
                .HasForeignKey(prt => prt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Warehouse>()
                .HasOne(w => w.Manager)
                .WithMany(u=>u.ManagedWarehouses)
                .HasForeignKey(w => w.ManagerId)
                .OnDelete(DeleteBehavior.SetNull);

            // SİSTEME İLK ADMİN HESABINI GÖMME (SEEDING)
            var adminId = Guid.NewGuid();

            modelBuilder.Entity<User>().HasData(new User
            {
                Id = adminId,
                FirstName = "System",
                LastName = "Admin",
                Email = "string",
                PasswordHash = "string",

                // BURASI DEĞİŞTİ: Artık string değil, Enum kullanıyoruz.
                Role = UserRole.SuperAdmin,

                CreatedDate = DateTime.UtcNow,
                IsActive = true
            });
        }
    }
}