using Microsoft.EntityFrameworkCore;
using Notifications.Domain.Notification;

namespace Notifications.Infrastructure.Persistence;

public class NotificationRepository(NotificationDbContext db) : INotificationRepository
{
    public Task<Notification?> GetByIdAsync(string id, CancellationToken ct = default) =>
        db.Notifications.FirstOrDefaultAsync(n => n.NotificationId == id, ct);

    public Task<Notification?> GetBySourceEventIdAsync(string sourceEventId, CancellationToken ct = default) =>
        db.Notifications.FirstOrDefaultAsync(n => n.SourceEventId == sourceEventId, ct);

    public async Task<IReadOnlyList<Notification>> GetAllAsync(CancellationToken ct = default) =>
        await db.Notifications.OrderByDescending(n => n.CreatedAtUtc).ToListAsync(ct);

    public async Task AddAsync(Notification notification, CancellationToken ct = default)
    {
        await db.Notifications.AddAsync(notification, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Notification notification, CancellationToken ct = default)
    {
        db.Notifications.Update(notification);
        await db.SaveChangesAsync(ct);
    }
}