using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Common.Notifications.Queries;
using Maba.Domain.Common;

namespace Maba.Application.Features.Common.Notifications.Handlers;

public class GetUnreadNotificationsCountQueryHandler : IRequestHandler<GetUnreadNotificationsCountQuery, int>
{
    private readonly IApplicationDbContext _context;

    public GetUnreadNotificationsCountQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(GetUnreadNotificationsCountQuery request, CancellationToken cancellationToken)
    {
        var count = await _context.Set<Notification>()
            .CountAsync(n => (n.UserId == request.UserId || n.UserId == null) && !n.IsRead, cancellationToken);

        return count;
    }
}

