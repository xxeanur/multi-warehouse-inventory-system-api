using Microsoft.EntityFrameworkCore;
using MultiWarehouse.Entity.Entities.Common;
using MultiWarehouse.Entity.Entities.Definitions;
using MultiWarehouse.Entity.Entities.Documents;
using MultiWarehouse.Entity.Entities.Identity;
using MultiWarehouse.Entity.Entities.Inventory;
using MultiWarehouse.Entity.Entities.Notification;

namespace MultiWarehouse.Service.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Tüm Tablolar
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<WarehouseZone> WarehouseZones { get; set; }
        public DbSet<Shelf> Shelves { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<InventoryCount> InventoryCounts { get; set; }
        public DbSet<InventoryCountDetail> InventoryCountDetails { get; set; }

        // WMS (FİŞ) Tabloları
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<InboundOrder> InboundOrders { get; set; }
        public DbSet<InboundOrderLine> InboundOrderLines { get; set; }
        public DbSet<OutboundOrder> OutboundOrders { get; set; }
        public DbSet<OutboundOrderLine> OutboundOrderLines { get; set; }
        public DbSet<OutboundOrderReservation> OutboundOrderReservations { get; set; }

        public DbSet<TransferOrder> TransferOrders { get; set; }
        public DbSet<TransferOrderLine> TransferOrderLines { get; set; }

        //Transfer Rezervasyon Tablosu
        public DbSet<TransferOrderReservation> TransferOrderReservations { get; set; }


        // Auth & Güvenlik
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Stock>()
                .HasIndex(s => new { s.WarehouseId, s.ProductId, s.ShelfId })
                .IsUnique();

            // İlişkiler (Foreign Keys)
            modelBuilder.Entity<Shelf>()
                .HasOne(s => s.WarehouseZone)
                .WithMany(wz => wz.Shelves)
                .HasForeignKey(s => s.WarehouseZoneId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WarehouseZone>()
                .HasOne(wz => wz.Warehouse)
                .WithMany(w => w.WarehouseZones)
                .HasForeignKey(wz => wz.WarehouseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Stock>()
                .HasOne(s => s.Product)
                .WithMany(p => p.Stocks)
                .HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PasswordResetToken>()
                .HasOne(prt => prt.User)
                .WithMany(u => u.PasswordResetTokens)
                .HasForeignKey(prt => prt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Warehouse>()
                .HasOne(w => w.Manager)
                .WithMany(u => u.ManagedWarehouses)
                .HasForeignKey(w => w.ManagerId)
                .OnDelete(DeleteBehavior.SetNull);

            //// SİSTEME İLK ADMİN HESABINI GÖMME (SEEDING)
            //var adminId = Guid.NewGuid();
            //modelBuilder.Entity<User>().HasData(new User
            //{
            //    Id = adminId,
            //    FirstName = "Esra Nur",
            //    LastName = "Çomak",
            //    Email = "str@gmail.com",
            //    PasswordHash = "$2a$11$43rRIxdH7vsTMJRC4zoXv.dntNlaWZ1yqcu1QDW7rtuymihDzgmWm",
            //    Role = UserRole.SuperAdmin,
            //    CreatedDate = DateTime.UtcNow,
            //    IsActive = true
            //});

            // ==========================================
            // 1. INBOUND ORDER İLİŞKİLERİ
            // ==========================================
            modelBuilder.Entity<InboundOrder>()
                .HasIndex(i => i.DocumentNumber)
                .IsUnique();

            modelBuilder.Entity<InboundOrder>()
                .HasOne(i => i.Warehouse)
                .WithMany()
                .HasForeignKey(i => i.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InboundOrder>()
                .HasOne(i => i.Supplier)
                .WithMany()
                .HasForeignKey(i => i.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InboundOrder>()
                .HasMany(i => i.Lines)
                .WithOne(l => l.InboundOrder)
                .HasForeignKey(l => l.InboundOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // ==========================================
            // 2. OUTBOUND ORDER İLİŞKİLERİ
            // ==========================================
            modelBuilder.Entity<OutboundOrder>()
                .HasIndex(o => o.DocumentNumber)
                .IsUnique();

            modelBuilder.Entity<OutboundOrder>()
                .HasOne(o => o.Warehouse)
                .WithMany()
                .HasForeignKey(o => o.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OutboundOrder>()
                .HasMany(o => o.Lines)
                .WithOne(l => l.OutboundOrder)
                .HasForeignKey(l => l.OutboundOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OutboundOrderReservation>()
                .HasOne(r => r.OutboundOrder)
                .WithMany(o => o.Reservations)
                .HasForeignKey(r => r.OutboundOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OutboundOrderReservation>()
                .HasOne(r => r.Product)
                .WithMany()
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OutboundOrderReservation>()
                .HasOne(r => r.Shelf)
                .WithMany()
                .HasForeignKey(r => r.ShelfId)
                .OnDelete(DeleteBehavior.Restrict);

            // ==========================================
            // 3. TRANSFER ORDER İLİŞKİLERİ
            // ==========================================
            modelBuilder.Entity<TransferOrder>()
                .HasIndex(t => t.DocumentNumber)
                .IsUnique();

            modelBuilder.Entity<TransferOrder>()
                .HasOne(t => t.SourceWarehouse)
                .WithMany()
                .HasForeignKey(t => t.SourceWarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransferOrder>()
                .HasOne(t => t.TargetWarehouse)
                .WithMany()
                .HasForeignKey(t => t.TargetWarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransferOrder>()
                .HasMany(t => t.Lines)
                .WithOne(l => l.TransferOrder)
                .HasForeignKey(l => l.TransferOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // TRANSFER ORDER RESERVATION İLİŞKİLERİ
            modelBuilder.Entity<TransferOrderReservation>()
                .HasOne(r => r.TransferOrder)
                .WithMany(o => o.Reservations)
                .HasForeignKey(r => r.TransferOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TransferOrderReservation>()
                .HasOne(r => r.Product)
                .WithMany()
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransferOrderReservation>()
                .HasOne(r => r.SourceShelf)
                .WithMany()
                .HasForeignKey(r => r.SourceShelfId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================================================
            // 4. MUHASEBE DEFTERİ (STOCK MOVEMENT) İLİŞKİLERİ
            // =========================================================
            modelBuilder.Entity<StockMovement>()
                .HasOne(s => s.Warehouse)
                .WithMany()
                .HasForeignKey(s => s.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockMovement>()
                .HasOne(s => s.Shelf)
                .WithMany()
                .HasForeignKey(s => s.ShelfId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockMovement>()
                .HasOne(s => s.Product)
                .WithMany()
                .HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}