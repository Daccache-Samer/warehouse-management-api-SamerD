using MediatR;
using Notifications.Domain.Notification;

namespace Notifications.Application.Notification.Commands.MarkNotificationAsRead;

public class MarkNotificationAsReadHandler(INotificationRepository repository)
    : IRequestHandler<MarkNotificationAsReadCommand, bool>
{
    public async Task<bool> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await repository.GetByIdAsync(request.NotificationId, cancellationToken);
        if (notification is null) return false;

        notification.MarkAsRead();
        await repository.UpdateAsync(notification, cancellationToken);
        return true;
    }
}