using Warehouse.DomainWarehouse.Domain.Exceptions;
namespace Warehouse.DomainWarehouse.Domain.Products;
public enum StockMovementType
{
    Inbound,
    Outbound,
    Adjustment
}
public class StockMovement
{
    public string Id { get; private set; } = string.Empty;
    public string ProductId { get; private set; } = string.Empty;
    public StockMovementType Type { get; private set; }
    public int Quantity { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public string? Reason { get; private set; }

    private StockMovement() { }

    public static StockMovement Create(
        string productId,
        StockMovementType type,
        int quantity,
        string? reason = null)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new DomainException("ProductId is required.");

        if (quantity <= 0)
            throw new DomainException("Movement quantity must be greater than zero.");

        return new StockMovement
        {
            Id = Guid.NewGuid().ToString(),
            ProductId = productId,
            Type = type,
            Quantity = quantity,
            OccurredAt = DateTime.UtcNow,
            Reason = reason
        };
    }
}