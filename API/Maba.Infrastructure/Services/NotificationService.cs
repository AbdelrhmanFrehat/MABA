using MediatR;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Common.Notifications.Commands;
using Maba.Application.Features.Common.Notifications.DTOs;

namespace Maba.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IMediator _mediator;

    public NotificationService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<NotificationDto> CreateNotificationAsync(Guid? userId, string type, string titleEn, string titleAr, string messageEn, string messageAr, string? linkUrl = null, Guid? relatedEntityId = null, string? relatedEntityType = null, CancellationToken cancellationToken = default)
    {
        var command = new CreateNotificationCommand
        {
            UserId = userId,
            Type = type,
            TitleEn = titleEn,
            TitleAr = titleAr,
            MessageEn = messageEn,
            MessageAr = messageAr,
            LinkUrl = linkUrl,
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = relatedEntityType
        };

        return await _mediator.Send(command, cancellationToken);
    }

    public async Task SendBroadcastNotificationAsync(string type, string titleEn, string titleAr, string messageEn, string messageAr, string? linkUrl = null, CancellationToken cancellationToken = default)
    {
        var command = new CreateNotificationCommand
        {
            UserId = null, // Broadcast
            Type = type,
            TitleEn = titleEn,
            TitleAr = titleAr,
            MessageEn = messageEn,
            MessageAr = messageAr,
            LinkUrl = linkUrl
        };

        await _mediator.Send(command, cancellationToken);
    }
}

