using MediatR;
using Notifications.Application.Notification.ViewModels;
using Notifications.Domain.Notification;

namespace Notifications.Application.Notification.Queries.ListNotifications;

public record ListNotificationsQuery (
    NotificationType? Type = null, NotificationSeverity? Severity = null, NotificationStatus? Status = null)
    : IRequest<IReadOnlyList<NotificationViewModel>>;