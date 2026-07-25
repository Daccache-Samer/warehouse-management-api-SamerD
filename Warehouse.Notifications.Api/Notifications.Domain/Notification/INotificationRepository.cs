using Notifications.Domain.Common;

namespace Notifications.Domain.Notification;

public interface INotificationRepository : IRepository<Notification>
{
    Task<Notification?> GetBySourceEventIdAsync(string sourceEventId, CancellationToken ct = default);
    Task<IReadOnlyList<Notification>> GetFilteredAsync(
        NotificationType? type, NotificationSeverity? severity, NotificationStatus? status,
        CancellationToken ct = default);
}