namespace Notifications.Application.IntegrationEvents;

public record IntegrationEventEnvelope
{
    public string EventId { get; init; } = string.Empty;
    public DateTime OccurredAtUtc { get; init; }
    public string CorrelationId { get; init; } = string.Empty;
    public string EventType { get; init; } = string.Empty;
    public string RelatedEntityId { get; init; } = string.Empty;
    public string RelatedEntityType { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
}