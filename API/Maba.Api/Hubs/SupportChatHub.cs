using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Maba.Domain.SupportChat;
using Maba.Infrastructure.Data;
using System.Security.Claims;

namespace Maba.Api.Hubs;

[Authorize]
public class SupportChatHub : Hub
{
    private readonly ApplicationDbContext _context;

    public SupportChatHub(ApplicationDbContext context)
    {
        _context = context;
    }

    private Guid GetUserId()
    {
        var value = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new HubException("User not authenticated.");
        return Guid.Parse(value);
    }

    private bool IsAdminOrStoreOwner()
    {
        return Context.User?.IsInRole("Admin") == true || Context.User?.IsInRole("StoreOwner") == true;
    }

    /// <summary>Customer: join my open conversation. Group = conversation Id.</summary>
    public async Task JoinMyConversation()
    {
        var userId = GetUserId();
        if (IsAdminOrStoreOwner()) return;

        var conv = await _context.SupportConversations
            .FirstOrDefaultAsync(c => c.CustomerId == userId && c.Status == SupportConversationStatus.Open);
        if (conv != null)
            await Groups.AddToGroupAsync(Context.ConnectionId, conv.Id.ToString());
    }

    /// <summary>Admin/StoreOwner: join a specific conversation to receive/send messages.</summary>
    public async Task JoinConversation(Guid conversationId)
    {
        if (!IsAdminOrStoreOwner()) throw new HubException("Forbidden.");
        var conv = await _context.SupportConversations.FindAsync(conversationId);
        if (conv == null) throw new HubException("Conversation not found.");
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
    }

    /// <summary>Send a message; validate, save, then broadcast ReceiveMessage to the conversation group. Content can be empty if attachment is provided.</summary>
    public async Task SendMessage(Guid conversationId, string content, string? attachmentUrl = null, string? attachmentFileName = null)
    {
        var hasContent = !string.IsNullOrWhiteSpace(content);
        var hasAttachment = !string.IsNullOrWhiteSpace(attachmentUrl);
        if (!hasContent && !hasAttachment)
            throw new HubException("Message must have content or an attachment.");
        if (hasContent && content!.Length > 4000)
            throw new HubException("Message content too long.");
        if (attachmentUrl?.Length > 2048 == true || attachmentFileName?.Length > 512 == true)
            throw new HubException("Invalid attachment data.");

        var userId = GetUserId();
        var conv = await _context.SupportConversations
            .Include(c => c.Customer)
            .Include(c => c.AssignedToUser)
            .FirstOrDefaultAsync(c => c.Id == conversationId);
        if (conv == null) throw new HubException("Conversation not found.");

        var isCustomer = conv.CustomerId == userId;
        var isStaff = IsAdminOrStoreOwner();
        if (!isCustomer && !isStaff) throw new HubException("Forbidden.");

        var message = new SupportMessage
        {
            ConversationId = conversationId,
            SenderUserId = userId,
            Content = content?.Trim() ?? "",
            AttachmentUrl = attachmentUrl,
            AttachmentFileName = attachmentFileName
        };
        _context.SupportMessages.Add(message);
        conv.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var senderName = (isCustomer ? conv.Customer : conv.AssignedToUser)?.FullName ?? "User";
        await Clients.Group(conversationId.ToString()).SendAsync("ReceiveMessage", new
        {
            message.Id,
            message.ConversationId,
            message.SenderUserId,
            SenderName = senderName,
            IsFromCustomer = isCustomer,
            message.Content,
            message.AttachmentUrl,
            message.AttachmentFileName,
            message.CreatedAt,
            message.ReadAt
        });
    }
}
