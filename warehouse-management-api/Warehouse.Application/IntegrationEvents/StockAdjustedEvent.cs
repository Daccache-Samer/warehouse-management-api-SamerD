namespace Warehouse.Application.IntegrationEvents;

public sealed record StockAdjustedEvent : IntegrationEvent
{
    public string Sku { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public string AdjustmentType { get; init; } = string.Empty; 
    public int Delta { get; init; }
    public int NewQuantity { get; init; }
    public string? Reason { get; init; }
}