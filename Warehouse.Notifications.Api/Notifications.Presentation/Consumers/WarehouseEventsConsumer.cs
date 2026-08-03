using MediatR;
using Notifications.Application.Common;
using Notifications.Application.Notification.Commands.ProcessWarehouseEvent;

namespace Notifications.Presentation.Consumers;

public class WarehouseEventsConsumer(IMediator mediator) : IWarehouseEventConsumer
{
    public Task<EventProcessingResult> HandleAsync(
        string routingKey, string payload, CancellationToken cancellationToken = default) =>
        mediator.Send(new ProcessWarehouseEventCommand(routingKey, payload), cancellationToken);
}