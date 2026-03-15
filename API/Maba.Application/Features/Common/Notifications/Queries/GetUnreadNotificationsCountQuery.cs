using MediatR;

namespace Maba.Application.Features.Common.Notifications.Queries;

public class GetUnreadNotificationsCountQuery : IRequest<int>
{
    public Guid UserId { get; set; }
}

