using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Common.Notifications.Commands;
using Maba.Application.Features.Common.Notifications.DTOs;
using Maba.Domain.Common;

namespace Maba.Application.Features.Common.Notifications.Handlers;

public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, NotificationDto>
{
    private readonly IApplicationDbContext _context;

    public MarkNotificationAsReadCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationDto> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _context.Set<Notification>()
            .Include(n => n.User)
            .FirstOrDefaultAsync(n => n.Id == request.NotificationId, cancellationToken);

        if (notification == null)
        {
            throw new KeyNotFoundException("Notification not found.");
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return new NotificationDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            UserFullName = notification.User?.FullName,
            Type = notification.Type,
            TitleEn = notification.TitleEn,
            TitleAr = notification.TitleAr,
            MessageEn = notification.MessageEn,
            MessageAr = notification.MessageAr,
            LinkUrl = notification.LinkUrl,
            Icon = notification.Icon,
            IsRead = notification.IsRead,
            ReadAt = notification.ReadAt,
            RelatedEntityId = notification.RelatedEntityId,
            RelatedEntityType = notification.RelatedEntityType,
            CreatedAt = notification.CreatedAt
        };
    }
}

