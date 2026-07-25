namespace Warehouse.Application.IntegrationEvents;

public sealed record StockLowDetectedEvent : IntegrationEvent
{
    public string Sku { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public int CurrentQuantity { get; init; }
    public int Threshold { get; init; }
}