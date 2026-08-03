namespace Notifications.Application.IntegrationEvents;

public sealed record WarehouseFileUploadedEvent : IntegrationEventEnvelope
{
    public string FileName { get; init; } = string.Empty;
    public string ObjectKey { get; init; } = string.Empty;
}