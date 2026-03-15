using MediatR;
using Maba.Application.Features.Common.Notifications.DTOs;

namespace Maba.Application.Features.Common.Notifications.Commands;

public class CreateNotificationCommand : IRequest<NotificationDto>
{
    public Guid? UserId { get; set; } // null = broadcast
    public string Type { get; set; } = "Info";
    public string TitleEn { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string MessageEn { get; set; } = string.Empty;
    public string MessageAr { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public string? Icon { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
}

