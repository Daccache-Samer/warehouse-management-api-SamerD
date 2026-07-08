using Warehouse.DomainWarehouse.Domain.Exceptions;
namespace Warehouse.DomainWarehouse.Domain.Products;

public class WarehouseItem
{
    public string Id { get; private set; } = string.Empty;
    public string ProductId { get; private set; } = string.Empty;
    public string Location { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public DateTime LastUpdatedAt { get; private set; }

    private WarehouseItem() { }

    public static WarehouseItem Create(string productId, string location, int quantity)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new DomainException("ProductId is required.");

        if (string.IsNullOrWhiteSpace(location))
            throw new DomainException("Location is required.");

        if (quantity < 0)
            throw new DomainException("Quantity cannot be negative.");

        return new WarehouseItem
        {
            Id = Guid.NewGuid().ToString(),
            ProductId = productId,
            Location = location,
            Quantity = quantity,
            LastUpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity < 0)
            throw new DomainException("Quantity cannot be negative.");

        Quantity = newQuantity;
        LastUpdatedAt = DateTime.UtcNow;
    }
}