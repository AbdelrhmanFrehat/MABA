using MediatR;
using Maba.Application.Common.Models;
using Maba.Application.Features.Common.Notifications.DTOs;

namespace Maba.Application.Features.Common.Notifications.Queries;

public class GetNotificationsQuery : IRequest<PagedResult<NotificationDto>>
{
    public Guid? UserId { get; set; }
    public string? Type { get; set; }
    public bool? IsRead { get; set; }
    public string? RelatedEntityType { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

