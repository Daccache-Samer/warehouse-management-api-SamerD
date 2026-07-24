using Microsoft.EntityFrameworkCore;
using Warehouse.DomainWarehouse.Domain.Products;
using Warehouse.DomainWarehouse.Domain.Suppliers;
namespace Warehouse.Infrastructure.Persistence;

public class WarehouseDbContext(DbContextOptions<WarehouseDbContext> options) : DbContext(options)
{
    public DbSet<Product>  Products { get; set; }
    public DbSet<Supplier>  Suppliers { get; set; }
    public DbSet<ProductImage>  ProductImages { get; set; }
    public DbSet<SupplierDocument> SupplierDocuments { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasOne<Supplier>()//Setup SupplierId as a ForeignKey in products
                .WithMany()
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasMany(typeof(ProductImage), "Images")
                .WithOne()
                .HasForeignKey("ProductId")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey("ProductId", "FileName"); //ProductImage has no ID field and no key, so I created a composite key 
        });
        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasMany(typeof(SupplierDocument), "Documents")
                .WithOne()
                .HasForeignKey("SupplierId")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SupplierDocument>(entity =>
        {
            entity.HasKey(d => d.SupplierDocumentId);
        });
    }
}
