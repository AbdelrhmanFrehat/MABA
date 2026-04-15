using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Maba.Application.Common.Interfaces;
using Maba.Application.Common.ServiceRequests;
using Maba.Domain.DesignCad;
using Maba.Domain.Users;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/design-cad-requests")]
[Authorize]
public class DesignCadRequestsController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DesignCadRequestsController> _logger;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".webp", ".pdf",
        ".step", ".stp", ".iges", ".igs", ".sldprt", ".sldasm", ".stl", ".obj", ".dxf", ".dwg", ".zip"
    };

    private const long MaxFileSizeBytes = 50 * 1024 * 1024; // 50 MB
    private const int MaxFiles = 20;

    public DesignCadRequestsController(
        IApplicationDbContext context,
        IFileStorageService fileStorage,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<DesignCadRequestsController> logger)
    {
        _context = context;
        _fileStorage = fileStorage;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Create a new Design CAD request with optional file uploads (multipart/form-data).
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<DesignCadRequestDto>> Create(
        [FromForm] string requestType,
        [FromForm] string title,
        [FromForm] string? description,
        [FromForm] string? targetProcess,
        [FromForm] string? intendedUse,
        [FromForm] string? materialNotes,
        [FromForm] string? dimensionsNotes,
        [FromForm] string? toleranceNotes,
        [FromForm] string? whatNeedsChange,
        [FromForm] string? criticalSurfaces,
        [FromForm] string? fitmentRequirements,
        [FromForm] string? purposeAndConstraints,
        [FromForm] string? deadline,
        [FromForm] bool hasPhysicalPart,
        [FromForm] bool legalConfirmation,
        [FromForm] bool canDeliverPhysicalPart,
        [FromForm] string? customerNotes,
        [FromForm] IFormFileCollection? files)
    {
        if (string.IsNullOrWhiteSpace(title))
            return BadRequest("Title is required.");

        if (!TryParseRequestType(requestType, out var requestTypeEnum))
            return BadRequest("Invalid request type.");

        if ((requestTypeEnum == DesignCadRequestType.ReverseEngineering || requestTypeEnum == DesignCadRequestType.PhysicalItem) && !legalConfirmation)
            return BadRequest("Legal confirmation is required for reverse engineering / physical item requests.");

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Guid? userId = null;
        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var parsedUserId))
            userId = parsedUserId;

        User? user = null;
        if (userId.HasValue)
            user = await _context.Set<User>().FindAsync(userId.Value);

        DesignCadTargetProcess? targetProcessEnum = null;
        if (!string.IsNullOrWhiteSpace(targetProcess) && Enum.TryParse<DesignCadTargetProcess>(targetProcess, true, out var tp))
            targetProcessEnum = tp;

        var referenceNumber = $"DCAD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";

        var request = new DesignCadServiceRequest
        {
            Id = Guid.NewGuid(),
            ReferenceNumber = referenceNumber,
            UserId = userId,
            RequestType = requestTypeEnum,
            Title = title.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            TargetProcess = targetProcessEnum,
            IntendedUse = string.IsNullOrWhiteSpace(intendedUse) ? null : intendedUse.Trim(),
            MaterialNotes = string.IsNullOrWhiteSpace(materialNotes) ? null : materialNotes.Trim(),
            DimensionsNotes = string.IsNullOrWhiteSpace(dimensionsNotes) ? null : dimensionsNotes.Trim(),
            ToleranceNotes = string.IsNullOrWhiteSpace(toleranceNotes) ? null : toleranceNotes.Trim(),
            WhatNeedsChange = string.IsNullOrWhiteSpace(whatNeedsChange) ? null : whatNeedsChange.Trim(),
            CriticalSurfaces = string.IsNullOrWhiteSpace(criticalSurfaces) ? null : criticalSurfaces.Trim(),
            FitmentRequirements = string.IsNullOrWhiteSpace(fitmentRequirements) ? null : fitmentRequirements.Trim(),
            PurposeAndConstraints = string.IsNullOrWhiteSpace(purposeAndConstraints) ? null : purposeAndConstraints.Trim(),
            Deadline = string.IsNullOrWhiteSpace(deadline) ? null : deadline.Trim(),
            HasPhysicalPart = hasPhysicalPart,
            LegalConfirmation = legalConfirmation,
            CanDeliverPhysicalPart = canDeliverPhysicalPart,
            CustomerNotes = string.IsNullOrWhiteSpace(customerNotes) ? null : customerNotes.Trim(),
            Status = DesignCadRequestStatus.Pending,
            CustomerName = user?.FullName,
            CustomerEmail = user?.Email,
            CustomerPhone = user?.Phone
        };

        _context.Set<DesignCadServiceRequest>().Add(request);

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
                    filePath = await _fileStorage.SaveFileAsync(stream, storedName, file.ContentType ?? "application/octet-stream", "design-cad-requests");

                var attachment = new DesignCadServiceRequestAttachment
                {
                    Id = Guid.NewGuid(),
                    RequestId = request.Id,
                    FileName = file.FileName,
                    FilePath = filePath,
                    FileSizeBytes = file.Length,
                    ContentType = file.ContentType ?? "application/octet-stream",
                    UploadedAt = DateTime.UtcNow
                };
                _context.Set<DesignCadServiceRequestAttachment>().Add(attachment);
            }
        }

        await _context.SaveChangesAsync(CancellationToken.None);

        try
        {
            await _emailService.SendRequestConfirmationAsync(request.CustomerEmail, request.CustomerName, referenceNumber, "Design CAD Request", null, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send confirmation email for Design CAD request {ReferenceNumber}", referenceNumber);
        }

        var created = await _context.Set<DesignCadServiceRequest>()
            .Include(r => r.User)
            .Include(r => r.Attachments)
            .FirstAsync(r => r.Id == request.Id);
        return CreatedAtAction(nameof(GetById), new { id = request.Id }, MapToDto(created));
    }

    private static bool TryParseRequestType(string rawRequestType, out DesignCadRequestType requestType)
    {
        if (Enum.TryParse<DesignCadRequestType>(rawRequestType, true, out requestType))
            return true;

        // Support common frontend aliases for backward compatibility.
        var normalized = rawRequestType?.Trim().Replace("-", "").Replace("_", "").Replace(" ", "").ToLowerInvariant();
        return normalized switch
        {
            "existingcad" => Assign(DesignCadRequestType.ExistingFiles, out requestType),
            "physicalobject" => Assign(DesignCadRequestType.PhysicalItem, out requestType),
            _ => false
        };
    }

    private static bool Assign(DesignCadRequestType value, out DesignCadRequestType requestType)
    {
        requestType = value;
        return true;
    }

    /// <summary>
    /// Get current user's requests (or all for admin).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<DesignCadRequestListDto>> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? requestType = null,
        [FromQuery] string? search = null)
    {
        var isAdmin = User.IsInRole("Admin") || User.IsInRole("Manager");
        var query = _context.Set<DesignCadServiceRequest>()
            .Include(r => r.User)
            .Include(r => r.Attachments)
            .AsQueryable();

        if (!isAdmin)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Ok(new DesignCadRequestListDto { Items = new List<DesignCadRequestDto>(), TotalCount = 0, Page = page, PageSize = pageSize });
            query = query.Where(r => r.UserId == userId);
        }

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<DesignCadRequestStatus>(status, true, out var statusEnum))
            query = query.Where(r => r.Status == statusEnum);

        if (!string.IsNullOrEmpty(requestType) && Enum.TryParse<DesignCadRequestType>(requestType, true, out var typeEnum))
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
            .Select(r => new DesignCadRequestDto
            {
                Id = r.Id,
                ReferenceNumber = r.ReferenceNumber,
                RequestType = r.RequestType.ToString(),
                Title = r.Title,
                Description = r.Description,
                Status = r.Status.ToString(),
                WorkflowStatus = ServiceRequestWorkflowMapper.FromDesignCad(r.Status).ToString(),
                CustomerName = r.CustomerName ?? (r.User != null ? r.User.FullName : null),
                CustomerEmail = r.CustomerEmail ?? (r.User != null ? r.User.Email : null),
                CreatedAt = r.CreatedAt,
                AttachmentCount = r.Attachments.Count
            })
            .ToListAsync();

        return Ok(new DesignCadRequestListDto { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize });
    }

    /// <summary>
    /// Get a single request by ID (own or admin).
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<DesignCadRequestDto>> GetById(Guid id)
    {
        var request = await _context.Set<DesignCadServiceRequest>()
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

        return Ok(MapToDto(request));
    }

    /// <summary>
    /// Update request status (admin/manager).
    /// </summary>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<DesignCadRequestDto>> UpdateStatus(Guid id, [FromBody] UpdateDesignCadStatusDto dto)
    {
        var request = await _context.Set<DesignCadServiceRequest>()
            .Include(r => r.User)
            .Include(r => r.Attachments)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null)
            return NotFound();

        if (!Enum.TryParse<DesignCadRequestStatus>(dto.Status, true, out var statusEnum))
            return BadRequest("Invalid status.");

        if (statusEnum == DesignCadRequestStatus.Rejected &&
            string.IsNullOrWhiteSpace(dto.RejectionReason))
            return BadRequest("A rejection reason is required when rejecting a request.");

        var previousStatus = request.Status;
        request.Status = statusEnum;

        if (statusEnum == DesignCadRequestStatus.UnderReview)
            request.ReviewedAt = DateTime.UtcNow;
        if (statusEnum == DesignCadRequestStatus.Completed)
            request.CompletedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(dto.Notes))
            request.AdminNotes = (request.AdminNotes ?? "") + "\n" + dto.Notes.Trim();
        if (!string.IsNullOrWhiteSpace(dto.RejectionReason))
            request.RejectionReason = dto.RejectionReason.Trim();

        await _context.SaveChangesAsync(CancellationToken.None);

        if (statusEnum != previousStatus)
        {
            var toEmail  = request.CustomerEmail ?? request.User?.Email;
            var custName = request.CustomerName  ?? request.User?.FullName;
            var baseUrl  = _configuration["App:FrontendBaseUrl"]?.TrimEnd('/') ?? "http://localhost:4200";
            var viewUrl  = $"{baseUrl}/account";

            if (statusEnum == DesignCadRequestStatus.Cancelled)
                await _emailService.SendRequestCancelledAsync(
                    toEmail, custName, request.ReferenceNumber,
                    "Design & CAD Request", viewUrl, dto.Notes, CancellationToken.None);
            else
                await _emailService.SendRequestStatusUpdateAsync(
                    toEmail, custName, request.ReferenceNumber,
                    "Design & CAD Request", statusEnum.ToString(), viewUrl,
                    dto.RejectionReason, CancellationToken.None);
        }

        return Ok(MapToDto(request));
    }

    /// <summary>
    /// Download a single attachment (owner or admin).
    /// </summary>
    [HttpGet("{id}/attachments/{attachmentId}")]
    public async Task<IActionResult> DownloadAttachment(Guid id, Guid attachmentId)
    {
        var request = await _context.Set<DesignCadServiceRequest>().FindAsync(id);
        if (request == null)
            return NotFound();

        var isAdmin = User.IsInRole("Admin") || User.IsInRole("Manager");
        if (!isAdmin)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId) || request.UserId != userId)
                return Forbid();
        }

        var attachment = await _context.Set<DesignCadServiceRequestAttachment>()
            .FirstOrDefaultAsync(a => a.Id == attachmentId && a.RequestId == id);
        if (attachment == null)
            return NotFound();

        var stream = await _fileStorage.GetFileAsync(attachment.FilePath);
        if (stream == null)
            return NotFound("File not found in storage.");

        return File(stream, attachment.ContentType, attachment.FileName);
    }

    private static DesignCadRequestDto MapToDto(DesignCadServiceRequest r)
    {
        return new DesignCadRequestDto
        {
            Id = r.Id,
            ReferenceNumber = r.ReferenceNumber,
            RequestType = r.RequestType.ToString(),
            Title = r.Title,
            Description = r.Description,
            TargetProcess = r.TargetProcess?.ToString(),
            IntendedUse = r.IntendedUse,
            MaterialNotes = r.MaterialNotes,
            DimensionsNotes = r.DimensionsNotes,
            ToleranceNotes = r.ToleranceNotes,
            WhatNeedsChange = r.WhatNeedsChange,
            CriticalSurfaces = r.CriticalSurfaces,
            FitmentRequirements = r.FitmentRequirements,
            PurposeAndConstraints = r.PurposeAndConstraints,
            Deadline = r.Deadline,
            HasPhysicalPart = r.HasPhysicalPart,
            LegalConfirmation = r.LegalConfirmation,
            CanDeliverPhysicalPart = r.CanDeliverPhysicalPart,
            CustomerNotes = r.CustomerNotes,
            AdminNotes = r.AdminNotes,
            QuotedPrice = r.QuotedPrice,
            FinalPrice = r.FinalPrice,
            Status = r.Status.ToString(),
            WorkflowStatus = ServiceRequestWorkflowMapper.FromDesignCad(r.Status).ToString(),
            ReviewedAt = r.ReviewedAt,
            CompletedAt = r.CompletedAt,
            CustomerName = r.CustomerName ?? (r.User != null ? r.User.FullName : null),
            CustomerEmail = r.CustomerEmail ?? (r.User != null ? r.User.Email : null),
            CustomerPhone = r.CustomerPhone ?? r.User?.Phone,
            CreatedAt = r.CreatedAt,
            AttachmentCount = r.Attachments.Count,
            Attachments = r.Attachments.Select(a => new DesignCadAttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                FileSizeBytes = a.FileSizeBytes,
                UploadedAt = a.UploadedAt
            }).ToList()
        };
    }

    private static string SanitizeFileName(string fileName)
    {
        var name = Path.GetFileName(fileName);
        if (string.IsNullOrEmpty(name)) return "file";
        name = Regex.Replace(name, @"[^\w\.\-]", "_");
        return name.Length > 100 ? name[..100] : name;
    }
}

public class DesignCadRequestDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = "";
    public string RequestType { get; set; } = "";
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string? TargetProcess { get; set; }
    public string? IntendedUse { get; set; }
    public string? MaterialNotes { get; set; }
    public string? DimensionsNotes { get; set; }
    public string? ToleranceNotes { get; set; }
    public string? WhatNeedsChange { get; set; }
    public string? CriticalSurfaces { get; set; }
    public string? FitmentRequirements { get; set; }
    public string? PurposeAndConstraints { get; set; }
    public string? Deadline { get; set; }
    public bool HasPhysicalPart { get; set; }
    public bool LegalConfirmation { get; set; }
    public bool CanDeliverPhysicalPart { get; set; }
    public string? CustomerNotes { get; set; }
    public string? AdminNotes { get; set; }
    public decimal? QuotedPrice { get; set; }
    public decimal? FinalPrice { get; set; }
    public string Status { get; set; } = "";
    public string WorkflowStatus { get; set; } = "";
    public DateTime? ReviewedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public DateTime CreatedAt { get; set; }
    public int AttachmentCount { get; set; }
    public List<DesignCadAttachmentDto> Attachments { get; set; } = new();
}

public class DesignCadAttachmentDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = "";
    public long FileSizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }
}

public class DesignCadRequestListDto
{
    public List<DesignCadRequestDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class UpdateDesignCadStatusDto
{
    public string Status { get; set; } = "";
    public string? Notes { get; set; }
    public string? RejectionReason { get; set; }
}
