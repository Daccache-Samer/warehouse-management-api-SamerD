using MediatR;
using Microsoft.AspNetCore.Mvc;
using Notifications.Application.Notification.Commands.MarkNotificationAsRead;
using Notifications.Application.Notification.Queries.GetNotificationById;
using Notifications.Application.Notification.Queries.ListNotifications;
using Notifications.Domain.Notification;

namespace Notifications.Presentation.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] NotificationType? type,
        [FromQuery] NotificationSeverity? severity,
        [FromQuery] NotificationStatus? status,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListNotificationsQuery(), cancellationToken);
        return Ok(result);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetNotificationByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkAsRead(string id, CancellationToken cancellationToken)
    {
        var found = await mediator.Send(new MarkNotificationAsReadCommand(id), cancellationToken);
        return found ? NoContent() : NotFound();
    }
}