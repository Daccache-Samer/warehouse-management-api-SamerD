namespace Notifications.Application.Notification.ViewModels;

public record NotificationViewModel(
    string NotificationId,
    string SourceEventId,
    string Type,
    string Title,
    string Message,
    string Severity,
    string RelatedEntityId,
    string RelatedEntityType,
    string Status);