using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Maba.Application.Features.Common.Notifications.Commands;
using Maba.Application.Features.Common.Notifications.Queries;
using Maba.Application.Common.Models;
using NotificationDto = Maba.Application.Features.Common.Notifications.DTOs.NotificationDto;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<NotificationDto>>> GetNotifications(
        [FromQuery] Guid? userId,
        [FromQuery] string? type,
        [FromQuery] bool? isRead,
        [FromQuery] string? relatedEntityType,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetNotificationsQuery
        {
            UserId = userId,
            Type = type,
            IsRead = isRead,
            RelatedEntityType = relatedEntityType,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadNotificationsCount([FromQuery] Guid userId)
    {
        var query = new GetUnreadNotificationsCountQuery { UserId = userId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<NotificationDto>> CreateNotification([FromBody] CreateNotificationCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetNotifications), new { }, result);
    }

    [HttpPost("{id}/read")]
    public async Task<ActionResult<NotificationDto>> MarkNotificationAsRead(Guid id)
    {
        var command = new MarkNotificationAsReadCommand { NotificationId = id };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("read-all")]
    public async Task<ActionResult> MarkAllNotificationsAsRead([FromQuery] Guid userId)
    {
        var command = new MarkAllNotificationsAsReadCommand { UserId = userId };
        await _mediator.Send(command);
        return NoContent();
    }
}

