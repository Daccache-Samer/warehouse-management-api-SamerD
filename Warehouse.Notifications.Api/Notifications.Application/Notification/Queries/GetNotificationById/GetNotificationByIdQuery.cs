using MediatR;
using Notifications.Application.Notification.ViewModels;

namespace Notifications.Application.Notification.Queries.GetNotificationById;

public record GetNotificationByIdQuery(string NotificationId) : IRequest<NotificationViewModel?>;