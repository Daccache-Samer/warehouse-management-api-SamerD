using AutoMapper;
using MediatR;
using Notifications.Application.Notification.ViewModels;
using Notifications.Domain.Notification;

namespace Notifications.Application.Notification.Queries.GetNotificationById;

public class GetNotificationByIdHandler(INotificationRepository repository, IMapper mapper)
    : IRequestHandler<GetNotificationByIdQuery, NotificationViewModel?>
{
    public async Task<NotificationViewModel?> Handle(GetNotificationByIdQuery request, CancellationToken cancellationToken)
    {
        var notification = await repository.GetByIdAsync(request.NotificationId, cancellationToken);
        return notification is null ? null : mapper.Map<NotificationViewModel>(notification);
    }
}