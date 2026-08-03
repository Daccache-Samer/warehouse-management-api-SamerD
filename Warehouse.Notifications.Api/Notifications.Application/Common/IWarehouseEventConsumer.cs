using Notifications.Application.Notification.Commands.ProcessWarehouseEvent;

namespace Notifications.Application.Common;

public interface IWarehouseEventConsumer
{
    Task<EventProcessingResult> HandleAsync(string routingKey, string payload, CancellationToken cancellationToken = default);
}