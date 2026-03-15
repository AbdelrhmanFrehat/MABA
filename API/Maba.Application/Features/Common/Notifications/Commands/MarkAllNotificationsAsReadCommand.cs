using MediatR;

namespace Maba.Application.Features.Common.Notifications.Commands;

public class MarkAllNotificationsAsReadCommand : IRequest<Unit>
{
    public Guid UserId { get; set; }
}

