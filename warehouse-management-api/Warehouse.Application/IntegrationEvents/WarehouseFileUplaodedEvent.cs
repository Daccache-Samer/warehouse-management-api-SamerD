namespace Warehouse.Application.IntegrationEvents;

public sealed record WarehouseFileUploadedEvent : IntegrationEvent
{
    public string FileName { get; init; } = string.Empty;
    public string ObjectKey { get; init; } = string.Empty;
}