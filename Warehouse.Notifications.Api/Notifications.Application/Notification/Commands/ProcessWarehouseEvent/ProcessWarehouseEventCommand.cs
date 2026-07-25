using MediatR;

namespace Notifications.Application.Notification.Commands.ProcessWarehouseEvent;

public enum EventProcessingResult { Processed, Duplicate, Unrecognized }

public record ProcessWarehouseEventCommand(string RoutingKey, string Payload) : IRequest<EventProcessingResult>;