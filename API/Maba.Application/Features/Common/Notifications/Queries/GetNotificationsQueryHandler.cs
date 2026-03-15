using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Common.Models;
using Maba.Application.Features.Common.Notifications.Queries;
using Maba.Application.Features.Common.Notifications.DTOs;
using Maba.Domain.Common;

namespace Maba.Application.Features.Common.Notifications.Handlers;

public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, PagedResult<NotificationDto>>
{
    private readonly IApplicationDbContext _context;

    public GetNotificationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<NotificationDto>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<Notification>()
            .Include(n => n.User)
            .AsQueryable();

        if (request.UserId.HasValue)
        {
            query = query.Where(n => n.UserId == request.UserId.Value);
        }
        else
        {
            // If no user specified, only return broadcast notifications (UserId == null)
            query = query.Where(n => n.UserId == null);
        }

        if (!string.IsNullOrWhiteSpace(request.Type))
        {
            query = query.Where(n => n.Type == request.Type);
        }

        if (request.IsRead.HasValue)
        {
            query = query.Where(n => n.IsRead == request.IsRead.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.RelatedEntityType))
        {
            query = query.Where(n => n.RelatedEntityType == request.RelatedEntityType);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var notificationsDto = notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            UserId = n.UserId,
            UserFullName = n.User?.FullName,
            Type = n.Type,
            TitleEn = n.TitleEn,
            TitleAr = n.TitleAr,
            MessageEn = n.MessageEn,
            MessageAr = n.MessageAr,
            LinkUrl = n.LinkUrl,
            Icon = n.Icon,
            IsRead = n.IsRead,
            ReadAt = n.ReadAt,
            RelatedEntityId = n.RelatedEntityId,
            RelatedEntityType = n.RelatedEntityType,
            CreatedAt = n.CreatedAt
        }).ToList();

        return new PagedResult<NotificationDto>
        {
            Items = notificationsDto,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

