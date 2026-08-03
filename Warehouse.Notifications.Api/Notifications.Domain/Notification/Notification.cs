using Notifications.Domain.Exceptions;

namespace Notifications.Domain.Notification;

public enum NotificationType
{
    StockLow,
    StockDepleted,
    WarehouseFileUploaded,
    ProductCreated,
    StockAdjusted
}

public enum NotificationSeverity
{
    Info,
    Warning,
    Critical
}
public enum NotificationStatus
{
    Unread,
    Read,
    Failed
}
public class Notification
{
    public string NotificationId { get; private set; } = string.Empty;
    public string SourceEventId { get; private set; } = string.Empty;
    public NotificationType Type { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public NotificationSeverity Severity { get; private set; }
    public string RelatedEntityId { get; private set; } = string.Empty;
    public string RelatedEntityType { get; private set; } = string.Empty;
    public NotificationStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? ReadAtUtc { get; private set; }
    
    private Notification() { }
    
    public static Notification Create(
        string sourceEventId,
        NotificationType type,
        string title,
        string message,
        NotificationSeverity severity,
        string relatedEntityId,
        string relatedEntityType)
    {
        if (string.IsNullOrWhiteSpace(sourceEventId))
            throw new DomainException("Source event id is required.");

        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title is required.");

        if (string.IsNullOrWhiteSpace(message))
            throw new DomainException("Message is required.");

        if (string.IsNullOrWhiteSpace(relatedEntityId))
            throw new DomainException("Related entity id is required.");

        if (string.IsNullOrWhiteSpace(relatedEntityType))
            throw new DomainException("Related entity type is required.");

        return new Notification
        {
            NotificationId = Guid.NewGuid().ToString(),
            SourceEventId = sourceEventId,
            Type = type,
            Title = title,
            Message = message,
            Severity = severity,
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = relatedEntityType,
            Status = NotificationStatus.Unread,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void MarkAsRead()
    {
        if (Status == NotificationStatus.Read) return; 

        Status = NotificationStatus.Read;
        ReadAtUtc = DateTime.UtcNow;
    }
    public static Notification CreateFailed(
        string sourceEventId, NotificationType type, string relatedEntityId, string relatedEntityType, string reason)
    {
        if (string.IsNullOrWhiteSpace(sourceEventId))
            throw new DomainException("Source event id is required.");

        return new Notification
        {
            NotificationId = Guid.NewGuid().ToString(),
            SourceEventId = sourceEventId,
            Type = type,
            Title = "Failed to process event",
            Message = reason,
            Severity = NotificationSeverity.Critical,
            RelatedEntityId = string.IsNullOrWhiteSpace(relatedEntityId) ? "unknown" : relatedEntityId,
            RelatedEntityType = string.IsNullOrWhiteSpace(relatedEntityType) ? "unknown" : relatedEntityType,
            Status = NotificationStatus.Failed,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
