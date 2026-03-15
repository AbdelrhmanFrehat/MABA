using MediatR;
using Maba.Application.Features.Common.Notifications.DTOs;

namespace Maba.Application.Features.Common.Notifications.Commands;

public class MarkNotificationAsReadCommand : IRequest<NotificationDto>
{
    public Guid NotificationId { get; set; }
}

