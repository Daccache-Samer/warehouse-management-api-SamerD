namespace Warehouse.Application.IntegrationEvents;

public sealed record ProductCreatedEvent : IntegrationEvent
{
    public string Sku { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int InitialQuantity { get; init; }
}