namespace Warehouse.Application.IntegrationEvents;

public abstract record IntegrationEvent
{
    public string EventId { get; init; } = Guid.NewGuid().ToString();
    public DateTime OccurredAtUtc { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = string.Empty;
    public string EventType { get; init; } = string.Empty;
    public string RelatedEntityId { get; init; } = string.Empty;
    public string RelatedEntityType { get; init; } = string.Empty;
    public string Severity { get; init; } = "Info";
}