using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Common.Notifications.Commands;
using Maba.Application.Features.Common.Notifications.DTOs;
using Maba.Domain.Common;

namespace Maba.Application.Features.Common.Notifications.Handlers;

public class CreateNotificationCommandHandler : IRequestHandler<CreateNotificationCommand, NotificationDto>
{
    private readonly IApplicationDbContext _context;

    public CreateNotificationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationDto> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Type = request.Type,
            TitleEn = request.TitleEn,
            TitleAr = request.TitleAr,
            MessageEn = request.MessageEn,
            MessageAr = request.MessageAr,
            LinkUrl = request.LinkUrl,
            Icon = request.Icon,
            RelatedEntityId = request.RelatedEntityId,
            RelatedEntityType = request.RelatedEntityType,
            IsRead = false
        };

        _context.Set<Notification>().Add(notification);
        await _context.SaveChangesAsync(cancellationToken);

        var user = request.UserId.HasValue
            ? await _context.Set<Domain.Users.User>()
                .FirstOrDefaultAsync(u => u.Id == request.UserId.Value, cancellationToken)
            : null;

        return new NotificationDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            UserFullName = user?.FullName,
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

