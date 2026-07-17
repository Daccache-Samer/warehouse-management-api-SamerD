using Warehouse.DomainWarehouse.Domain.Exceptions;
using Warehouse.DomainWarehouse.Domain.Suppliers;

namespace Warehouse.DomainWarehouse.Domain.Products;

public class Product
{
    public string Id { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string SKU { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int QuantityInStock { get; private set; }
    public string? SupplierId { get; private set; }
    public DateTime ExpiryDate { get; private set; }
    public bool IsArchived { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime LastUpdatedAt { get; private set; }

    private readonly List<ProductImage> _images = new();
    public IReadOnlyList<ProductImage> Images => _images.AsReadOnly();

    private Product() { } // reserved for future ORM materialization (Challenge 3)

    public static Product Create(
        string name,
        string sku,
        string description,
        decimal price,
        int quantityInStock,
        DateTime expiryDate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name is required.");

        if (string.IsNullOrWhiteSpace(sku))
            throw new DomainException("SKU is required.");

        if (price <= 0)
            throw new DomainException("Price must be greater than zero.");

        if (quantityInStock < 0)
            throw new DomainException("Quantity cannot be negative.");

        return new Product
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            SKU = sku,
            Description = description,
            Price = price,
            QuantityInStock = quantityInStock,
            ExpiryDate = expiryDate,
            IsArchived = false,
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdatePrice(decimal newPrice)
    {
        EnsureNotArchived();

        if (newPrice <= 0)
            throw new DomainException("Price must be greater than zero.");

        Price = newPrice;
        LastUpdatedAt = DateTime.UtcNow;
    }

    public void UpdateQuantity(int newQuantity)
    {
        EnsureNotArchived();

        if (newQuantity < 0)
            throw new DomainException("Quantity cannot be negative.");

        QuantityInStock = newQuantity;
        LastUpdatedAt = DateTime.UtcNow;
    }

    public void Archive()
    {
        if (IsArchived) return; // idempotent — archiving twice is a no-op, not an error

        IsArchived = true;
        LastUpdatedAt = DateTime.UtcNow;
    }

    public void AssignSupplier(Supplier supplier)
    {
        EnsureNotArchived();

        if (supplier is null)
            throw new DomainException("Supplier is required.");

        if (!supplier.IsActive)
            throw new DomainException("Inactive suppliers cannot be assigned to products.");

        SupplierId = supplier.SupplierId;
        LastUpdatedAt = DateTime.UtcNow;
    }

    public void AddImage(ProductImage image)
    {
        EnsureNotArchived();
        _images.Add(image);
        LastUpdatedAt = DateTime.UtcNow;
    }

    private void EnsureNotArchived()
    {
        if (IsArchived)
            throw new DomainException("Archived products cannot be updated.");
    }
    public void AdjustQuantity(int delta)
    {
        EnsureNotArchived();

        var newQuantity = QuantityInStock + delta;
        if (newQuantity < 0)
            throw new DomainException("Stock adjustment would result in negative quantity.");

        QuantityInStock = newQuantity;
        LastUpdatedAt = DateTime.UtcNow;
    }
}