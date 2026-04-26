using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Maba.Infrastructure.Data;
using Maba.Domain.SupportChat;
using Maba.Api.DTOs.SupportChat;
using Maba.Application.Common.Interfaces;
using System.Security.Claims;
using Maba.Api.Services;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/support-conversations")]
[Authorize]
public class SupportConversationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly SupportChatMessagingService _messaging;
    private readonly AdminNotificationService _adminNotify;
    private readonly ILogger<SupportConversationsController> _logger;

    public SupportConversationsController(
        ApplicationDbContext context,
        IFileStorageService fileStorage,
        SupportChatMessagingService messaging,
        AdminNotificationService adminNotify,
        ILogger<SupportConversationsController> logger)
    {
        _context = context;
        _fileStorage = fileStorage;
        _messaging = messaging;
        _adminNotify = adminNotify;
        _logger = logger;
    }

    private Guid GetUserId()
    {
        var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated.");
        return Guid.Parse(value);
    }

    private bool IsAdminOrStoreOwner()
    {
        return User.IsInRole("Admin") || User.IsInRole("StoreOwner") || User.IsInRole("Manager");
    }

    /// <summary>Customer: list my conversations (newest first). Does not create rows.</summary>
    [HttpGet("mine")]
    public async Task<ActionResult<List<SupportConversationDto>>> GetMine(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (IsAdminOrStoreOwner())
            return Ok(new List<SupportConversationDto>());

        var list = await _context.SupportConversations
            .AsNoTracking()
            .Include(c => c.Customer)
            .Include(c => c.AssignedToUser)
            .Where(c => c.CustomerId == userId)
            .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
            .ToListAsync(cancellationToken);

        var lastMessages = await GetLastMessagesAsync(list.Select(c => c.Id).ToList(), cancellationToken);

        var dtos = list.Select(c =>
        {
            var hasLast = lastMessages.TryGetValue(c.Id, out var last);
            return MapToDto(c, hasLast ? last.Content : null, hasLast ? last.CreatedAt : (DateTime?)null, 0);
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>Create a conversation for the current user (subject + optional first message). Works for any authenticated user; customer id is always taken from the token.</summary>
    [HttpPost]
    public async Task<ActionResult<SupportConversationDto>> Create(
        [FromBody] CreateSupportConversationRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null)
            return BadRequest("Request body is required.");

        var userId = GetUserId();

        var subject = string.IsNullOrWhiteSpace(request.Subject)
            ? "Support"
            : request.Subject.Trim();
        if (subject.Length > 200)
            return BadRequest("Subject is too long.");

        var conv = new SupportConversation
        {
            CustomerId = userId,
            Subject = subject,
            Status = SupportConversationStatus.Open,
            RelatedOrderId = request.RelatedOrderId,
            RelatedDesignId = request.RelatedDesignId
        };
        await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            _context.SupportConversations.Add(conv);
            await _context.SaveChangesAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(request.InitialMessage))
            {
                await _messaging.SendAsync(
                    conv.Id,
                    userId,
                    actingAsStaff: false,
                    request.InitialMessage,
                    null,
                    null,
                    cancellationToken);
            }

            await tx.CommitAsync(cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            await tx.RollbackAsync(cancellationToken);
            return Forbid();
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Support conversation create failed (database update).");
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    error = "Support chat could not be saved. The database may be missing recent migrations (SupportConversations.Subject).",
                    code = "support_conversation_db"
                });
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Support conversation create failed.");
            throw;
        }

        // Notify all admin/manager users — fire-and-forget, never blocks
        var customer = await _context.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new { u.FullName, u.Email })
            .FirstOrDefaultAsync(cancellationToken);

        _ = _adminNotify.NotifySupportChatAsync(
            customer?.FullName,
            customer?.Email,
            subject,
            request.InitialMessage);

        var newId = conv.Id;
        try
        {
            var reloaded = await _context.SupportConversations
                .Include(c => c.Customer)
                .Include(c => c.AssignedToUser)
                .FirstOrDefaultAsync(c => c.Id == newId, cancellationToken);

            if (reloaded == null)
            {
                _logger.LogError("Support conversation {ConversationId} not found after commit.", newId);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { error = "Conversation was saved but could not be reloaded. Try refreshing the conversation list.", code = "support_conversation_reload" });
            }

            var lastMsg = await _context.SupportMessages
                .AsNoTracking()
                .Where(m => m.ConversationId == newId)
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new { m.Content, m.CreatedAt })
                .FirstOrDefaultAsync(cancellationToken);

            return Ok(MapToDto(reloaded, lastMsg?.Content, lastMsg?.CreatedAt, 0));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Support conversation reload after create failed for {ConversationId}.", newId);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "Conversation may have been created. Refresh the page or open Support Chat again.", code = "support_conversation_reload" });
        }
    }

    /// <summary>Admin/StoreOwner/Manager: list all conversations.</summary>
    [HttpGet]
    public async Task<ActionResult<List<SupportConversationDto>>> GetAll(
        [FromQuery] SupportConversationStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (!IsAdminOrStoreOwner())
            return Forbid();

        var query = _context.SupportConversations
            .AsNoTracking()
            .Include(c => c.Customer)
            .Include(c => c.AssignedToUser)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        var list = await query
            .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var lastMessages = await GetLastMessagesAsync(list.Select(c => c.Id).ToList(), cancellationToken);

        var dtos = list.Select(c =>
        {
            var hasLast = lastMessages.TryGetValue(c.Id, out var last);
            return MapToDto(c, hasLast ? last.Content : null, hasLast ? last.CreatedAt : (DateTime?)null, 0);
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>Get messages for a conversation (customer: only own; staff: any).</summary>
    [HttpGet("{id:guid}/messages")]
    public async Task<ActionResult<List<SupportMessageDto>>> GetMessages(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var conv = await _context.SupportConversations.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (conv == null) return NotFound();

        if (conv.CustomerId != userId && !IsAdminOrStoreOwner())
            return Forbid();

        var messages = await _context.SupportMessages
            .Where(m => m.ConversationId == id)
            .Include(m => m.SenderUser)
            .OrderBy(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = messages.Select(m => new SupportMessageDto(
            m.Id,
            m.ConversationId,
            m.SenderUserId,
            m.SenderUser?.FullName,
            m.SenderUserId == conv.CustomerId,
            m.Content,
            m.AttachmentUrl,
            m.AttachmentFileName,
            m.CreatedAt,
            m.ReadAt
        )).ToList();

        return Ok(dtos);
    }

    /// <summary>Send a message (REST fallback when SignalR is unavailable).</summary>
    [HttpPost("{id:guid}/messages")]
    public async Task<ActionResult<SupportMessageDto>> PostMessage(
        Guid id,
        [FromBody] SendMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        try
        {
            var dto = await _messaging.SendAsync(
                id,
                userId,
                IsAdminOrStoreOwner(),
                request.Content,
                request.AttachmentUrl,
                request.AttachmentFileName,
                cancellationToken);
            return Ok(dto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>Customer or staff: close conversation.</summary>
    [HttpPost("{id:guid}/close")]
    public async Task<ActionResult> Close(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var conv = await _context.SupportConversations.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (conv == null) return NotFound();

        if (conv.CustomerId != userId && !IsAdminOrStoreOwner())
            return Forbid();

        conv.Status = SupportConversationStatus.Closed;
        conv.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return Ok();
    }

    /// <summary>Staff: reopen a closed conversation.</summary>
    [HttpPost("{id:guid}/reopen")]
    public async Task<ActionResult> Reopen(Guid id, CancellationToken cancellationToken = default)
    {
        if (!IsAdminOrStoreOwner())
            return Forbid();

        var conv = await _context.SupportConversations.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (conv == null) return NotFound();

        conv.Status = SupportConversationStatus.Open;
        conv.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return Ok();
    }

    /// <summary>Upload an attachment for a conversation. Returns URL and filename to send in a message.</summary>
    [HttpPost("{id:guid}/upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<UploadAttachmentResponse>> UploadAttachment(
        Guid id,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");
        if (file.Length > 10 * 1024 * 1024)
            return BadRequest("File size must be under 10 MB.");

        var userId = GetUserId();
        var conv = await _context.SupportConversations.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (conv == null) return NotFound();
        if (conv.Status != SupportConversationStatus.Open)
            return BadRequest("Conversation is closed.");
        if (conv.CustomerId != userId && !IsAdminOrStoreOwner())
            return Forbid();

        var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? "";
        var safeName = Path.GetFileName(file.FileName);
        if (string.IsNullOrWhiteSpace(safeName)) safeName = "attachment" + ext;

        string relativePath;
        await using (var stream = file.OpenReadStream())
        {
            relativePath = await _fileStorage.SaveFileAsync(stream, safeName, file.ContentType ?? "application/octet-stream", "support-chat");
        }
        var url = await _fileStorage.GetFileUrlAsync(relativePath);
        return Ok(new UploadAttachmentResponse(url, safeName));
    }

    /// <summary>Assign conversation to an admin user (optional).</summary>
    [HttpPost("{id:guid}/assign")]
    public async Task<ActionResult> Assign(Guid id, [FromQuery] Guid? assignToUserId, CancellationToken cancellationToken)
    {
        if (!IsAdminOrStoreOwner()) return Forbid();

        var conv = await _context.SupportConversations.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (conv == null) return NotFound();

        conv.AssignedToUserId = assignToUserId;
        conv.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return Ok();
    }

    private async Task<Dictionary<Guid, (string? Content, DateTime CreatedAt)>> GetLastMessagesAsync(
        List<Guid> conversationIds,
        CancellationToken cancellationToken)
    {
        if (conversationIds.Count == 0)
            return new Dictionary<Guid, (string?, DateTime)>();

        // Fetch the most recent message per conversation without filtered Include
        var rows = await _context.SupportMessages
            .AsNoTracking()
            .Where(m => conversationIds.Contains(m.ConversationId))
            .Select(m => new { m.ConversationId, m.Content, m.CreatedAt })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(m => m.ConversationId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var last = g.OrderByDescending(m => m.CreatedAt).First();
                    return (last.Content, last.CreatedAt);
                });
    }

    private static SupportConversationDto MapToDto(SupportConversation c, string? lastPreview, DateTime? lastAt, int unreadCount)
    {
        return new SupportConversationDto(
            c.Id,
            c.CustomerId,
            c.Customer?.FullName,
            c.Subject,
            c.AssignedToUserId,
            c.AssignedToUser?.FullName,
            c.Status,
            c.RelatedOrderId,
            c.RelatedDesignId,
            c.CreatedAt,
            c.UpdatedAt,
            lastPreview != null && lastPreview.Length > 80 ? lastPreview[..80] + "…" : lastPreview,
            lastAt,
            unreadCount
        );
    }
}
