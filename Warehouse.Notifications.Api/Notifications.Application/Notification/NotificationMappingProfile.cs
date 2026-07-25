using AutoMapper;
using Notifications.Application.Notification.ViewModels;

namespace Notifications.Application.Notification;

public class NotificationMappingProfile : Profile
{
    public NotificationMappingProfile()
    {
        CreateMap<Domain.Notification.Notification, NotificationViewModel>();
    }
}