using AutoMapper;
using MediatR;
using Notifications.Application.Notification.ViewModels;
using Notifications.Domain.Notification;

namespace Notifications.Application.Notification.Queries.ListNotifications;

public class ListNotificationsHandler(INotificationRepository repository, IMapper mapper)
    : IRequestHandler<ListNotificationsQuery, IReadOnlyList<NotificationViewModel>>
{
    public async Task<IReadOnlyList<NotificationViewModel>> Handle(
        ListNotificationsQuery request, CancellationToken cancellationToken)
    {
        var notifications = await repository.GetAllAsync(cancellationToken);
        return mapper.Map<IReadOnlyList<NotificationViewModel>>(notifications);
    }
}