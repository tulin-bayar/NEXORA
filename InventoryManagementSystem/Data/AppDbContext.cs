
using InventoryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // DATABASE TABLES

        public DbSet<Category> Categories { get; set; } = null!;

        public DbSet<Product> Products { get; set; } = null!;

        public DbSet<StockMovement> StockMovements { get; set; } = null!;
        public DbSet<Supplier> Suppliers { get; set; } = null!;

        public DbSet<ApplicationSetting> ApplicationSettings { get; set; } = null!;


        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // CATEGORY

            modelBuilder.Entity<Category>()
                .HasKey(c => c.CategoryId);

            modelBuilder.Entity<Category>()
                .Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            // PRODUCT

            modelBuilder.Entity<Product>()
                .HasKey(p => p.ProductId);

            modelBuilder.Entity<Product>()
                .Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(150);

            // CATEGORY - PRODUCT RELATIONSHIP

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // STOCK MOVEMENT

            modelBuilder.Entity<StockMovement>()
                .HasKey(m => m.MovementId);

            modelBuilder.Entity<StockMovement>()
                .Property(m => m.MovementType)
                .IsRequired()
                .HasMaxLength(20);

            modelBuilder.Entity<StockMovement>()
                .Property(m => m.Note)
                .HasMaxLength(500);

            // PRODUCT - STOCK MOVEMENT RELATIONSHIP

            modelBuilder.Entity<StockMovement>()
                .HasOne(m => m.Product)
                .WithMany()
                .HasForeignKey(m => m.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
