using System.Text.Json;
using MediatR;
using Notifications.Application.IntegrationEvents;
using Notifications.Domain.Exceptions;
using Notifications.Domain.Notification;

namespace Notifications.Application.Notification.Commands.ProcessWarehouseEvent;

public class ProcessWarehouseEventHandler(INotificationRepository repository, ILogger<ProcessWarehouseEventHandler> logger)
    : IRequestHandler<ProcessWarehouseEventCommand, EventProcessingResult>
{
    private static readonly HashSet<string> KnownRoutingKeys =
    [
        WarehouseEventTypes.StockLow, WarehouseEventTypes.StockAdjusted,
        WarehouseEventTypes.ProductCreated, WarehouseEventTypes.FileUploaded
    ];

    public async Task<EventProcessingResult> Handle(ProcessWarehouseEventCommand request, CancellationToken cancellationToken)
    {
        if (!KnownRoutingKeys.Contains(request.RoutingKey))
        {
            logger.LogWarning("Unrecognized routing key {RoutingKey}, dropping without a record", request.RoutingKey);
            return EventProcessingResult.Unrecognized;
        }

        IntegrationEventEnvelope envelope;
        try
        {
            envelope = JsonSerializer.Deserialize<IntegrationEventEnvelope>(request.Payload)
                       ?? throw new JsonException("Empty payload.");
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Unparseable message on routing key {RoutingKey}, dropping without a record", request.RoutingKey);
            return EventProcessingResult.Unrecognized;
        }

        if (string.IsNullOrWhiteSpace(envelope.EventId))
        {
            logger.LogError("Message on routing key {RoutingKey} has no EventId, dropping without a record", request.RoutingKey);
            return EventProcessingResult.Unrecognized;
        }

        if (await repository.GetBySourceEventIdAsync(envelope.EventId, cancellationToken) is not null)
        {
            logger.LogInformation("Duplicate delivery of event {EventId} ignored", envelope.EventId);
            return EventProcessingResult.Duplicate;
        }

        Domain.Notification.Notification notification;
        try
        {
            notification = request.RoutingKey switch
            {
                WarehouseEventTypes.StockLow => BuildStockLow(request.Payload),
                WarehouseEventTypes.StockAdjusted => BuildStockAdjusted(request.Payload),
                WarehouseEventTypes.ProductCreated => BuildProductCreated(request.Payload),
                WarehouseEventTypes.FileUploaded => BuildFileUploaded(request.Payload),
                _ => throw new InvalidOperationException("Unreachable — routing key already validated above.")
            };
        }
        catch (Exception ex) when (ex is JsonException or DomainException)
        {
            logger.LogError(ex, "Failed to process event {EventId} on routing key {RoutingKey}",
                envelope.EventId, request.RoutingKey);

            notification = Domain.Notification.Notification.CreateFailed(
                envelope.EventId, MapRoutingKeyToType(request.RoutingKey),
                envelope.RelatedEntityId, envelope.RelatedEntityType,
                $"Could not process {request.RoutingKey} event: {ex.Message}");

            await repository.AddAsync(notification, cancellationToken);
            return EventProcessingResult.Processed;
        }

        await repository.AddAsync(notification, cancellationToken);
        return EventProcessingResult.Processed;
    }

    private static NotificationType MapRoutingKeyToType(string routingKey) => routingKey switch
    {
        WarehouseEventTypes.StockLow => NotificationType.StockLow,
        WarehouseEventTypes.StockAdjusted => NotificationType.StockAdjusted,
        WarehouseEventTypes.ProductCreated => NotificationType.ProductCreated,
        WarehouseEventTypes.FileUploaded => NotificationType.WarehouseFileUploaded,
        _ => throw new InvalidOperationException("Unreachable — routing key already validated above.")
    };

    private static Domain.Notification.Notification BuildStockLow(string payload)
    {
        var e = JsonSerializer.Deserialize<StockLowDetectedEvent>(payload) ?? throw new JsonException("Empty payload.");
        var depleted = e.CurrentQuantity == 0;
        var type = depleted ? NotificationType.StockDepleted : NotificationType.StockLow;
        var severity = depleted ? NotificationSeverity.Critical : NotificationSeverity.Warning;
        var title = depleted ? $"Stock depleted: {e.ProductName}" : $"Low stock: {e.ProductName}";
        var message = depleted
            ? $"{e.ProductName} ({e.Sku}) is out of stock."
            : $"{e.ProductName} ({e.Sku}) is at {e.CurrentQuantity}, below the threshold of {e.Threshold}.";

        return Domain.Notification.Notification.Create(e.EventId, type, title, message, severity, e.RelatedEntityId, e.RelatedEntityType);
    }

    private static Domain.Notification.Notification BuildStockAdjusted(string payload)
    {
        var e = JsonSerializer.Deserialize<StockAdjustedEvent>(payload) ?? throw new JsonException("Empty payload.");
        var message = $"{e.ProductName} ({e.Sku}) {e.AdjustmentType.ToLowerInvariant()}d by {Math.Abs(e.Delta)} -> new quantity {e.NewQuantity}."
                       + (string.IsNullOrWhiteSpace(e.Reason) ? "" : $" Reason: {e.Reason}");

        return Domain.Notification.Notification.Create(
            e.EventId, NotificationType.StockAdjusted, $"Stock adjusted: {e.ProductName}", message,
            ParseSeverity(e.Severity), e.RelatedEntityId, e.RelatedEntityType);
    }

    private static Domain.Notification.Notification BuildProductCreated(string payload)
    {
        var e = JsonSerializer.Deserialize<ProductCreatedEvent>(payload) ?? throw new JsonException("Empty payload.");
        var message = $"{e.ProductName} ({e.Sku}) added with {e.InitialQuantity} units at {e.Price:C}.";

        return Domain.Notification.Notification.Create(
            e.EventId, NotificationType.ProductCreated, $"New product: {e.ProductName}", message,
            ParseSeverity(e.Severity), e.RelatedEntityId, e.RelatedEntityType);
    }

    private static Domain.Notification.Notification BuildFileUploaded(string payload)
    {
        var e = JsonSerializer.Deserialize<WarehouseFileUploadedEvent>(payload) ?? throw new JsonException("Empty payload.");
        var message = $"{e.FileName} was uploaded for {e.RelatedEntityType} {e.RelatedEntityId}.";

        return Domain.Notification.Notification.Create(
            e.EventId, NotificationType.WarehouseFileUploaded, $"File uploaded: {e.FileName}", message,
            ParseSeverity(e.Severity), e.RelatedEntityId, e.RelatedEntityType);
    }

    private static NotificationSeverity ParseSeverity(string severity) =>
        Enum.TryParse<NotificationSeverity>(severity, ignoreCase: true, out var parsed) ? parsed : NotificationSeverity.Info;
}