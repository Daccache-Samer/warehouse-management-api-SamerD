using MediatR;
using Notifications.Application.Notification.ViewModels;

namespace Notifications.Application.Notification.Queries.ListNotifications;

public record ListNotificationsQuery : IRequest<IReadOnlyList<NotificationViewModel>>;