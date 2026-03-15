using Maba.Application.Features.Common.Notifications.DTOs;

namespace Maba.Application.Common.Interfaces;

public interface INotificationService
{
    Task<NotificationDto> CreateNotificationAsync(Guid? userId, string type, string titleEn, string titleAr, string messageEn, string messageAr, string? linkUrl = null, Guid? relatedEntityId = null, string? relatedEntityType = null, CancellationToken cancellationToken = default);
    Task SendBroadcastNotificationAsync(string type, string titleEn, string titleAr, string messageEn, string messageAr, string? linkUrl = null, CancellationToken cancellationToken = default);
}

