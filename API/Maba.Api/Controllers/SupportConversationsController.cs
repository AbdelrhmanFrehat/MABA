using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Maba.Infrastructure.Data;
using Maba.Domain.SupportChat;
using Maba.Domain.Users;
using Maba.Api.DTOs.SupportChat;
using System.Security.Claims;
using Maba.Application.Common.Interfaces;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/support-conversations")]
[Authorize]
public class SupportConversationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;

    public SupportConversationsController(ApplicationDbContext context, IFileStorageService fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    private Guid GetUserId()
    {
        var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated.");
        return Guid.Parse(value);
    }

    private bool IsAdminOrStoreOwner()
    {
        return User.IsInRole("Admin") || User.IsInRole("StoreOwner");
    }

    /// <summary>Customer: get or create my conversation. Admin: not used.</summary>
    [HttpGet("mine")]
    public async Task<ActionResult<SupportConversationDto>> GetOrCreateMine(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var conv = await _context.SupportConversations
            .Include(c => c.Customer)
            .Include(c => c.AssignedToUser)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
            .FirstOrDefaultAsync(c => c.CustomerId == userId && c.Status == SupportConversationStatus.Open, cancellationToken);

        if (conv == null)
        {
            conv = new SupportConversation
            {
                CustomerId = userId,
                Status = SupportConversationStatus.Open
            };
            _context.SupportConversations.Add(conv);
            await _context.SaveChangesAsync(cancellationToken);
            conv = await _context.SupportConversations
                .Include(c => c.Customer)
                .Include(c => c.AssignedToUser)
                .FirstAsync(c => c.Id == conv.Id, cancellationToken);
        }

        var lastMsg = conv.Messages.FirstOrDefault();
        return Ok(MapToDto(conv, lastMsg?.Content, lastMsg?.CreatedAt, 0));
    }

    /// <summary>Admin/StoreOwner: list all conversations.</summary>
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
            .Include(c => c.Customer)
            .Include(c => c.AssignedToUser)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        var list = await query
            .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = list.Select(c =>
        {
            var last = c.Messages.FirstOrDefault();
            return MapToDto(c, last?.Content, last?.CreatedAt, 0);
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>Get messages for a conversation (customer: only own; admin: any).</summary>
    [HttpGet("{id:guid}/messages")]
    public async Task<ActionResult<List<SupportMessageDto>>> GetMessages(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var conv = await _context.SupportConversations.FindAsync([id], cancellationToken);
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

        var isCustomer = conv.CustomerId == userId;
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

    /// <summary>Upload an attachment (image or file) for a conversation. Returns URL and filename to send in a message.</summary>
    [HttpPost("{id:guid}/upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<UploadAttachmentResponse>> UploadAttachment(
        Guid id,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");
        if (file.Length > 10 * 1024 * 1024) // 10 MB
            return BadRequest("File size must be under 10 MB.");

        var userId = GetUserId();
        var conv = await _context.SupportConversations.FindAsync([id], cancellationToken);
        if (conv == null) return NotFound();
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

        var conv = await _context.SupportConversations.FindAsync([id], cancellationToken);
        if (conv == null) return NotFound();

        conv.AssignedToUserId = assignToUserId;
        conv.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return Ok();
    }

    private static SupportConversationDto MapToDto(SupportConversation c, string? lastPreview, DateTime? lastAt, int unreadCount)
    {
        return new SupportConversationDto(
            c.Id,
            c.CustomerId,
            c.Customer?.FullName,
            c.AssignedToUserId,
            c.AssignedToUser?.FullName,
            c.Status,
            c.CreatedAt,
            c.UpdatedAt,
            lastPreview != null && lastPreview.Length > 80 ? lastPreview[..80] + "…" : lastPreview,
            lastAt,
            unreadCount
        );
    }
}
