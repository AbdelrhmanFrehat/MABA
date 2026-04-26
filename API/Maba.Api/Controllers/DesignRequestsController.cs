using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Maba.Application.Common.Interfaces;
using Maba.Api.Services;
using Maba.Application.Common.ServiceRequests;
using Maba.Domain.Design;
using Maba.Domain.Users;
using System.IO.Compression;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/design-requests")]
[Authorize]
public class DesignRequestsController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly AdminNotificationService _adminNotify;
    private readonly ILogger<DesignRequestsController> _logger;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".webp", ".pdf",
        ".step", ".stp", ".iges", ".igs", ".sldprt", ".sldasm", ".stl", ".obj", ".dxf", ".zip"
    };

    private const long MaxFileSizeBytes = 50 * 1024 * 1024; // 50 MB
    private const int MaxFiles = 20;

    public DesignRequestsController(
        IApplicationDbContext context,
        IFileStorageService fileStorage,
        IEmailService emailService,
        IConfiguration configuration,
        AdminNotificationService adminNotify,
        ILogger<DesignRequestsController> logger)
    {
        _context = context;
        _fileStorage = fileStorage;
        _emailService = emailService;
        _configuration = configuration;
        _adminNotify = adminNotify;
        _logger = logger;
    }

    /// <summary>
    /// Create a new design request with optional file uploads (multipart/form-data).
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<DesignRequestDto>> Create(
        [FromForm] string requestType,
        [FromForm] string title,
        [FromForm] string? description,
        [FromForm] string? intendedUse,
        [FromForm] string? materialPreference,
        [FromForm] string? dimensionsNotes,
        [FromForm] string? toleranceLevel,
        [FromForm] string? budgetRange,
        [FromForm] string? timeline,
        [FromForm] bool ipOwnershipConfirmed,
        [FromForm] IFormFileCollection? files)
    {
        if (string.IsNullOrWhiteSpace(title))
            return BadRequest("Title is required.");

        if (!Enum.TryParse<DesignServiceRequestType>(requestType, true, out var requestTypeEnum))
            return BadRequest("Invalid request type.");

        if (requestTypeEnum == DesignServiceRequestType.PhysicalObject && !ipOwnershipConfirmed)
            return BadRequest("IP ownership confirmation is required for physical object requests.");

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Guid? userId = null;
        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var parsedUserId))
            userId = parsedUserId;

        User? user = null;
        if (userId.HasValue)
            user = await _context.Set<User>().FindAsync(userId.Value);

        var referenceNumber = $"DSG-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";

        var request = new DesignServiceRequest
        {
            Id = Guid.NewGuid(),
            ReferenceNumber = referenceNumber,
            UserId = userId,
            RequestType = requestTypeEnum,
            Title = title.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            IntendedUse = string.IsNullOrWhiteSpace(intendedUse) ? null : intendedUse.Trim(),
            MaterialPreference = string.IsNullOrWhiteSpace(materialPreference) ? null : materialPreference.Trim(),
            DimensionsNotes = string.IsNullOrWhiteSpace(dimensionsNotes) ? null : dimensionsNotes.Trim(),
            BudgetRange = string.IsNullOrWhiteSpace(budgetRange) ? null : budgetRange.Trim(),
            Timeline = string.IsNullOrWhiteSpace(timeline) ? null : timeline.Trim(),
            IpOwnershipConfirmed = ipOwnershipConfirmed,
            Status = DesignServiceRequestStatus.New,
            CustomerName = user?.FullName,
            CustomerEmail = user?.Email,
            CustomerPhone = user?.Phone
        };

        if (!string.IsNullOrWhiteSpace(toleranceLevel) && Enum.TryParse<ToleranceLevel>(toleranceLevel, true, out var tol))
            request.ToleranceLevel = tol;

        _context.Set<DesignServiceRequest>().Add(request);

        if (files != null && files.Count > 0)
        {
            if (files.Count > MaxFiles)
                return BadRequest($"Maximum {MaxFiles} files allowed.");

            foreach (var file in files)
            {
                if (file.Length == 0) continue;
                if (file.Length > MaxFileSizeBytes)
                    return BadRequest($"File {file.FileName} exceeds maximum size (50 MB).");

                var ext = Path.GetExtension(file.FileName);
                if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
                    return BadRequest($"File type not allowed: {file.FileName}");

                var safeName = SanitizeFileName(file.FileName);
                var storedName = $"{Guid.NewGuid()}_{safeName}";
                string filePath;
                await using (var stream = file.OpenReadStream())
                    filePath = await _fileStorage.SaveFileAsync(stream, storedName, file.ContentType ?? "application/octet-stream", "design-requests");

                var attachment = new DesignServiceRequestAttachment
                {
                    Id = Guid.NewGuid(),
                    RequestId = request.Id,
                    FileName = file.FileName,
                    FilePath = filePath,
                    FileSizeBytes = file.Length,
                    ContentType = file.ContentType ?? "application/octet-stream",
                    UploadedAt = DateTime.UtcNow
                };
                _context.Set<DesignServiceRequestAttachment>().Add(attachment);
            }
        }

        await _context.SaveChangesAsync(CancellationToken.None);

        try
        {
            var frontendBase = _configuration["App:FrontendBaseUrl"]?.TrimEnd('/') ?? "https://mabasol.com";
            await _emailService.SendRequestConfirmationAsync(request.CustomerEmail, request.CustomerName, referenceNumber, "Design Request", null, CancellationToken.None);
            _ = _adminNotify.NotifyNewRequestAsync(request.CustomerName, request.CustomerEmail, referenceNumber, "Design Request", $"{frontendBase}/admin/design-requests");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send confirmation email for Design request {ReferenceNumber}", referenceNumber);
        }

        var created = await _context.Set<DesignServiceRequest>()
            .Include(r => r.User)
            .Include(r => r.Attachments)
            .FirstAsync(r => r.Id == request.Id);
        var dto = await MapToDto(created);
        return CreatedAtAction(nameof(GetById), new { id = request.Id }, dto);
    }

    /// <summary>
    /// Get current user's design requests (or all for admin).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<DesignRequestListDto>> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? requestType = null,
        [FromQuery] string? search = null)
    {
        var isAdmin = User.IsInRole("Admin") || User.IsInRole("Manager");
        var query = _context.Set<DesignServiceRequest>()
            .Include(r => r.User)
            .Include(r => r.Attachments)
            .AsQueryable();

        if (!isAdmin)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Ok(new DesignRequestListDto { Items = new List<DesignRequestDto>(), TotalCount = 0, Page = page, PageSize = pageSize });
            query = query.Where(r => r.UserId == userId);
        }

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<DesignServiceRequestStatus>(status, true, out var statusEnum))
            query = query.Where(r => r.Status == statusEnum);

        if (!string.IsNullOrEmpty(requestType) && Enum.TryParse<DesignServiceRequestType>(requestType, true, out var typeEnum))
            query = query.Where(r => r.RequestType == typeEnum);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(r =>
                r.ReferenceNumber.Contains(term) ||
                (r.Title != null && r.Title.Contains(term)) ||
                (r.CustomerName != null && r.CustomerName.Contains(term)) ||
                (r.CustomerEmail != null && r.CustomerEmail.Contains(term)));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new DesignRequestDto
            {
                Id = r.Id,
                ReferenceNumber = r.ReferenceNumber,
                RequestType = r.RequestType.ToString(),
                Title = r.Title,
                Description = r.Description,
                Status = r.Status.ToString(),
                WorkflowStatus = ServiceRequestWorkflowMapper.FromDesign(r.Status).ToString(),
                CustomerName = r.CustomerName ?? (r.User != null ? r.User.FullName : null),
                CustomerEmail = r.CustomerEmail ?? (r.User != null ? r.User.Email : null),
                CustomerPhone = r.CustomerPhone ?? r.User!.Phone,
                CreatedAt = r.CreatedAt,
                AttachmentCount = r.Attachments.Count
            })
            .ToListAsync();

        return Ok(new DesignRequestListDto { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize });
    }

    /// <summary>
    /// Get a single design request by ID (own or admin).
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<DesignRequestDto>> GetById(Guid id)
    {
        var request = await _context.Set<DesignServiceRequest>()
            .Include(r => r.User)
            .Include(r => r.Attachments)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null)
            return NotFound();

        var isAdmin = User.IsInRole("Admin") || User.IsInRole("Manager");
        if (!isAdmin)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId) || request.UserId != userId)
                return Forbid();
        }

        return Ok(await MapToDto(request));
    }

    /// <summary>
    /// Download a single attachment.
    /// </summary>
    [HttpGet("{id}/attachments/{attachmentId}")]
    public async Task<IActionResult> DownloadAttachment(Guid id, Guid attachmentId)
    {
        var request = await _context.Set<DesignServiceRequest>().FindAsync(id);
        if (request == null)
            return NotFound();

        var isAdmin = User.IsInRole("Admin") || User.IsInRole("Manager");
        if (!isAdmin)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId) || request.UserId != userId)
                return Forbid();
        }

        var attachment = await _context.Set<DesignServiceRequestAttachment>()
            .FirstOrDefaultAsync(a => a.Id == attachmentId && a.RequestId == id);
        if (attachment == null)
            return NotFound();

        var stream = await _fileStorage.GetFileAsync(attachment.FilePath);
        if (stream == null)
            return NotFound("File not found in storage.");

        return File(stream, attachment.ContentType, attachment.FileName);
    }

    /// <summary>
    /// Download all attachments as a zip file (admin or request owner).
    /// </summary>
    [HttpGet("{id}/download-all")]
    public async Task<IActionResult> DownloadAll(Guid id)
    {
        var request = await _context.Set<DesignServiceRequest>()
            .Include(r => r.Attachments)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (request == null)
            return NotFound();

        var isAdmin = User.IsInRole("Admin") || User.IsInRole("Manager");
        if (!isAdmin)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId) || request.UserId != userId)
                return Forbid();
        }

        if (request.Attachments == null || request.Attachments.Count == 0)
            return NotFound("No attachments to download.");

        var zipStream = new MemoryStream();
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
        {
            var distinctNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var att in request.Attachments)
            {
                var fileStream = await _fileStorage.GetFileAsync(att.FilePath);
                if (fileStream == null) continue;
                var entryName = att.FileName;
                if (distinctNames.Contains(entryName))
                    entryName = $"{Path.GetFileNameWithoutExtension(att.FileName)}_{att.Id:N}{Path.GetExtension(att.FileName)}";
                distinctNames.Add(entryName);
                var entry = archive.CreateEntry(entryName, System.IO.Compression.CompressionLevel.Fastest);
                await using (var entryStream = entry.Open())
                await using (fileStream)
                    await fileStream.CopyToAsync(entryStream);
            }
        }

        zipStream.Position = 0;
        var zipName = $"design-request-{request.ReferenceNumber}-attachments.zip";
        return File(zipStream, "application/zip", zipName);
    }

    /// <summary>
    /// Update request status (admin/manager only).
    /// </summary>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<DesignRequestDto>> UpdateStatus(Guid id, [FromBody] UpdateDesignRequestStatusDto dto)
    {
        var request = await _context.Set<DesignServiceRequest>()
            .Include(r => r.User)
            .Include(r => r.Attachments)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (request == null)
            return NotFound();

        if (!Enum.TryParse<DesignServiceRequestStatus>(dto.Status, true, out var newStatus))
            return BadRequest("Invalid status.");

        if (newStatus == DesignServiceRequestStatus.Rejected &&
            string.IsNullOrWhiteSpace(dto.RejectionReason))
            return BadRequest("A rejection reason is required when rejecting a request.");

        var previousStatus = request.Status;
        request.Status = newStatus;
        request.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(dto.Notes))
            request.AdminNotes = (request.AdminNotes ?? "") + "\n" + dto.Notes.Trim();

        if (!string.IsNullOrWhiteSpace(dto.RejectionReason))
            request.RejectionReason = dto.RejectionReason.Trim();

        if (newStatus == DesignServiceRequestStatus.UnderReview && !request.ReviewedAt.HasValue)
            request.ReviewedAt = DateTime.UtcNow;
        if (newStatus == DesignServiceRequestStatus.Quoted && !request.QuotedAt.HasValue)
            request.QuotedAt = DateTime.UtcNow;
        if (newStatus == DesignServiceRequestStatus.Delivered && !request.DeliveredAt.HasValue)
            request.DeliveredAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(CancellationToken.None);

        if (newStatus != previousStatus)
        {
            var toEmail  = request.CustomerEmail ?? request.User?.Email;
            var custName = request.CustomerName  ?? request.User?.FullName;
            var baseUrl  = _configuration["App:FrontendBaseUrl"]?.TrimEnd('/') ?? "http://localhost:4200";
            var viewUrl  = $"{baseUrl}/account";

            if (newStatus == DesignServiceRequestStatus.Cancelled)
                await _emailService.SendRequestCancelledAsync(
                    toEmail, custName, request.ReferenceNumber,
                    "Design Request", viewUrl, dto.Notes, CancellationToken.None);
            else
                await _emailService.SendRequestStatusUpdateAsync(
                    toEmail, custName, request.ReferenceNumber,
                    "Design Request", newStatus.ToString(), viewUrl,
                    dto.RejectionReason, CancellationToken.None);
        }

        return Ok(await MapToDto(request));
    }

    /// <summary>
    /// Update admin fields: notes, quoted price, final price, delivery notes (admin/manager only).
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<DesignRequestDto>> Update(Guid id, [FromBody] UpdateDesignRequestDto dto)
    {
        var request = await _context.Set<DesignServiceRequest>()
            .Include(r => r.User)
            .Include(r => r.Attachments)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (request == null)
            return NotFound();

        if (dto.AdminNotes != null)
            request.AdminNotes = dto.AdminNotes;
        if (dto.QuotedPrice.HasValue)
            request.QuotedPrice = dto.QuotedPrice.Value;
        if (dto.FinalPrice.HasValue)
            request.FinalPrice = dto.FinalPrice.Value;
        if (dto.DeliveryNotes != null)
            request.DeliveryNotes = dto.DeliveryNotes;

        request.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(CancellationToken.None);
        return Ok(await MapToDto(request));
    }

    private static async Task<DesignRequestDto> MapToDto(DesignServiceRequest r)
    {
        var attachments = await Task.FromResult(r.Attachments?.Select(a => new DesignRequestAttachmentDto
        {
            Id = a.Id,
            FileName = a.FileName,
            FileSizeBytes = a.FileSizeBytes,
            ContentType = a.ContentType,
            UploadedAt = a.UploadedAt
        }).ToList() ?? new List<DesignRequestAttachmentDto>());

        return new DesignRequestDto
        {
            Id = r.Id,
            ReferenceNumber = r.ReferenceNumber,
            RequestType = r.RequestType.ToString(),
            Title = r.Title,
            Description = r.Description,
            IntendedUse = r.IntendedUse,
            MaterialPreference = r.MaterialPreference,
            DimensionsNotes = r.DimensionsNotes,
            ToleranceLevel = r.ToleranceLevel?.ToString(),
            BudgetRange = r.BudgetRange,
            Timeline = r.Timeline,
            IpOwnershipConfirmed = r.IpOwnershipConfirmed,
            Status = r.Status.ToString(),
            WorkflowStatus = ServiceRequestWorkflowMapper.FromDesign(r.Status).ToString(),
            CustomerName = r.CustomerName ?? r.User?.FullName,
            CustomerEmail = r.CustomerEmail ?? r.User?.Email,
            CustomerPhone = r.CustomerPhone ?? r.User?.Phone,
            AdminNotes = r.AdminNotes,
            QuotedPrice = r.QuotedPrice,
            FinalPrice = r.FinalPrice,
            DeliveryNotes = r.DeliveryNotes,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt,
            ReviewedAt = r.ReviewedAt,
            QuotedAt = r.QuotedAt,
            DeliveredAt = r.DeliveredAt,
            Attachments = attachments,
            AttachmentCount = attachments.Count
        };
    }

    private static string SanitizeFileName(string fileName)
    {
        var name = Path.GetFileName(fileName);
        if (string.IsNullOrEmpty(name)) name = "file";
        name = Regex.Replace(name, @"[^a-zA-Z0-9._-]", "_");
        return name.Length > 200 ? name[..200] : name;
    }
}

public class DesignRequestDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string RequestType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IntendedUse { get; set; }
    public string? MaterialPreference { get; set; }
    public string? DimensionsNotes { get; set; }
    public string? ToleranceLevel { get; set; }
    public string? BudgetRange { get; set; }
    public string? Timeline { get; set; }
    public bool IpOwnershipConfirmed { get; set; }
    public string Status { get; set; } = string.Empty;
    public string WorkflowStatus { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? AdminNotes { get; set; }
    public decimal? QuotedPrice { get; set; }
    public decimal? FinalPrice { get; set; }
    public string? DeliveryNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime? QuotedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public List<DesignRequestAttachmentDto> Attachments { get; set; } = new();
    public int AttachmentCount { get; set; }
}

public class DesignRequestAttachmentDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}

public class DesignRequestListDto
{
    public List<DesignRequestDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class UpdateDesignRequestStatusDto
{
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? RejectionReason { get; set; }
}

public class UpdateDesignRequestDto
{
    public string? AdminNotes { get; set; }
    public decimal? QuotedPrice { get; set; }
    public decimal? FinalPrice { get; set; }
    public string? DeliveryNotes { get; set; }
}
