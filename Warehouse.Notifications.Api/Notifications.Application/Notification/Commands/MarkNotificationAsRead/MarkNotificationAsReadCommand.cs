using MediatR;

namespace Notifications.Application.Notification.Commands.MarkNotificationAsRead;

public record MarkNotificationAsReadCommand(string NotificationId) : IRequest<bool>;