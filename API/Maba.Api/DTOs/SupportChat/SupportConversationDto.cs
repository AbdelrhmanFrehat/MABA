using Maba.Domain.SupportChat;

namespace Maba.Api.DTOs.SupportChat;

public record SupportConversationDto(
    Guid Id,
    Guid CustomerId,
    string? CustomerName,
    Guid? AssignedToUserId,
    string? AssignedToUserName,
    SupportConversationStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string? LastMessagePreview,
    DateTime? LastMessageAt,
    int UnreadCount
);

public record SupportMessageDto(
    Guid Id,
    Guid ConversationId,
    Guid SenderUserId,
    string? SenderName,
    bool IsFromCustomer,
    string Content,
    string? AttachmentUrl,
    string? AttachmentFileName,
    DateTime CreatedAt,
    DateTime? ReadAt
);

public record UploadAttachmentResponse(string Url, string FileName);

public record SendMessageRequest(string Content);
