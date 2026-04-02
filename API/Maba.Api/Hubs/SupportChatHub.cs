using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Maba.Infrastructure.Data;
using Maba.Api.Services;
using Maba.Api.DTOs.SupportChat;
using System.Security.Claims;

namespace Maba.Api.Hubs;

[Authorize]
public class SupportChatHub : Hub
{
    private readonly ApplicationDbContext _context;
    private readonly SupportChatMessagingService _messaging;

    public SupportChatHub(ApplicationDbContext context, SupportChatMessagingService messaging)
    {
        _context = context;
        _messaging = messaging;
    }

    private Guid GetUserId()
    {
        var value = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new HubException("User not authenticated.");
        return Guid.Parse(value);
    }

    private bool IsAdminOrStoreOwner()
    {
        return Context.User?.IsInRole("Admin") == true
            || Context.User?.IsInRole("StoreOwner") == true
            || Context.User?.IsInRole("Manager") == true;
    }

    /// <summary>Customer: join SignalR groups for all my conversations (receive messages on every thread).</summary>
    public async Task JoinMyConversations()
    {
        var userId = GetUserId();
        if (IsAdminOrStoreOwner()) return;

        var ids = await _context.SupportConversations
            .AsNoTracking()
            .Where(c => c.CustomerId == userId)
            .Select(c => c.Id)
            .ToListAsync();

        foreach (var id in ids)
            await Groups.AddToGroupAsync(Context.ConnectionId, id.ToString());
    }

    /// <summary>Join a specific conversation (customer must own it, or staff).</summary>
    public async Task JoinConversation(Guid conversationId)
    {
        var userId = GetUserId();
        var conv = await _context.SupportConversations.AsNoTracking().FirstOrDefaultAsync(c => c.Id == conversationId);
        if (conv == null) throw new HubException("Conversation not found.");

        var isCustomer = conv.CustomerId == userId;
        var isStaff = IsAdminOrStoreOwner();
        if (!isCustomer && !isStaff) throw new HubException("Forbidden.");

        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
    }

    /// <summary>Send a message; validate, save, then broadcast ReceiveMessage to the conversation group.</summary>
    public async Task SendMessage(Guid conversationId, string content, string? attachmentUrl = null, string? attachmentFileName = null)
    {
        var userId = GetUserId();
        SupportMessageDto dto;
        try
        {
            dto = await _messaging.SendAsync(
                conversationId,
                userId,
                IsAdminOrStoreOwner(),
                content,
                attachmentUrl,
                attachmentFileName,
                Context.ConnectionAborted);
        }
        catch (InvalidOperationException ex)
        {
            throw new HubException(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            throw new HubException("Forbidden.");
        }

        await Clients.Group(conversationId.ToString()).SendAsync("ReceiveMessage", new
        {
            dto.Id,
            dto.ConversationId,
            dto.SenderUserId,
            SenderName = dto.SenderName,
            dto.IsFromCustomer,
            dto.Content,
            dto.AttachmentUrl,
            dto.AttachmentFileName,
            dto.CreatedAt,
            dto.ReadAt
        });
    }
}
