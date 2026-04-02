using Microsoft.EntityFrameworkCore;
using Maba.Api.DTOs.SupportChat;
using Maba.Domain.SupportChat;
using Maba.Domain.Users;
using Maba.Infrastructure.Data;

namespace Maba.Api.Services;

/// <summary>Shared message send logic for REST API and SignalR hub (single code path).</summary>
public class SupportChatMessagingService
{
    private readonly ApplicationDbContext _context;

    public SupportChatMessagingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SupportMessageDto> SendAsync(
        Guid conversationId,
        Guid userId,
        bool actingAsStaff,
        string? content,
        string? attachmentUrl,
        string? attachmentFileName,
        CancellationToken cancellationToken = default)
    {
        var hasContent = !string.IsNullOrWhiteSpace(content);
        var hasAttachment = !string.IsNullOrWhiteSpace(attachmentUrl);
        if (!hasContent && !hasAttachment)
            throw new InvalidOperationException("Message must have content or an attachment.");
        if (hasContent && content!.Length > 4000)
            throw new InvalidOperationException("Message content too long.");
        if (attachmentUrl?.Length > 2048)
            throw new InvalidOperationException("Invalid attachment data.");
        if (attachmentFileName?.Length > 512)
            throw new InvalidOperationException("Invalid attachment data.");

        var conv = await _context.SupportConversations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == conversationId, cancellationToken)
            ?? throw new InvalidOperationException("Conversation not found.");

        if (conv.Status != SupportConversationStatus.Open)
            throw new InvalidOperationException("Conversation is closed.");

        var isCustomer = conv.CustomerId == userId;
        var isStaff = actingAsStaff;
        if (!isCustomer && !isStaff)
            throw new UnauthorizedAccessException("Forbidden.");

        var message = new SupportMessage
        {
            ConversationId = conversationId,
            SenderUserId = userId,
            Content = content?.Trim() ?? "",
            AttachmentUrl = attachmentUrl,
            AttachmentFileName = attachmentFileName
        };

        _context.SupportMessages.Add(message);

        var convTracked = await _context.SupportConversations.FirstAsync(c => c.Id == conversationId, cancellationToken);
        convTracked.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var sender = await _context.Set<User>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        return new SupportMessageDto(
            message.Id,
            message.ConversationId,
            message.SenderUserId,
            sender?.FullName,
            isCustomer,
            message.Content,
            message.AttachmentUrl,
            message.AttachmentFileName,
            message.CreatedAt,
            message.ReadAt);
    }
}
