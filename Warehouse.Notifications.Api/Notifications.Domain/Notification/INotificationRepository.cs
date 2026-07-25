using Notifications.Domain.Common;

namespace Notifications.Domain.Notification;

public interface INotificationRepository : IRepository<Notification>
{
    Task<Notification?> GetBySourceEventIdAsync(string sourceEventId, CancellationToken ct = default);
}