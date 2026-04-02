using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Printing;
using Maba.Domain.Media;
using Maba.Domain.Users;
using System.Security.Claims;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/3d-requests")]
[Authorize]
public class Print3dRequestsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IFileStorageService _fileStorageService;
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<Print3dRequestsController> _logger;

    public Print3dRequestsController(
        IMediator mediator,
        IFileStorageService fileStorageService,
        IApplicationDbContext context,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<Print3dRequestsController> logger)
    {
        _mediator = mediator;
        _fileStorageService = fileStorageService;
        _context = context;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Create a new 3D print request with file upload
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<Print3dRequestDto>> CreateRequest(
        IFormFile file, 
        [FromForm] string materialId,
        [FromForm] string? materialColorId,
        [FromForm] string? profileId,
        [FromForm] string? comments)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        if (string.IsNullOrEmpty(materialId) || !Guid.TryParse(materialId, out var materialGuid))
        {
            return BadRequest("Valid materialId is required");
        }

        // Get current user ID from claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Guid? userId = null;
        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        // Validate material exists
        var material = await _context.Set<Material>().FindAsync(materialGuid);
        if (material == null)
        {
            return BadRequest("Material not found");
        }

        // Parse optional materialColorId
        Guid? materialColorGuid = null;
        MaterialColor? selectedColor = null;
        if (!string.IsNullOrEmpty(materialColorId) && Guid.TryParse(materialColorId, out var parsedColorId))
        {
            materialColorGuid = parsedColorId;
            selectedColor = await _context.Set<MaterialColor>()
                .FirstOrDefaultAsync(c => c.Id == materialColorGuid && c.MaterialId == materialGuid && c.IsActive);
        }

        // Parse optional profileId
        Guid? profileGuid = null;
        PrintQualityProfile? profile = null;
        if (!string.IsNullOrEmpty(profileId) && Guid.TryParse(profileId, out var parsedProfileId))
        {
            profileGuid = parsedProfileId;
            profile = await _context.Set<PrintQualityProfile>().FindAsync(profileGuid);
        }

        // Save the file
        var fileExtension = Path.GetExtension(file.FileName);
        var savedFileName = $"{Guid.NewGuid()}{fileExtension}";
        
        string filePath;
        await using (var stream = file.OpenReadStream())
        {
            filePath = await _fileStorageService.SaveFileAsync(stream, savedFileName, file.ContentType, "3d-requests");
        }

        var fileUrl = await _fileStorageService.GetFileUrlAsync(filePath);

        // Get user info if authenticated
        User? user = null;
        if (userId.HasValue)
        {
            user = await _context.Set<User>().FindAsync(userId.Value);
        }

        // Generate reference number
        var referenceNumber = $"3DP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

        // If user is authenticated, also create a Design entry so the file appears in "My Designs"
        Guid? designId = null;
        if (userId.HasValue)
        {
            // Get or create a MediaType for 3D files
            var mediaType = await _context.Set<MediaType>()
                .FirstOrDefaultAsync(mt => mt.Key == "3D_MODEL") 
                ?? await _context.Set<MediaType>().FirstAsync();

            // Create MediaAsset
            var mediaAsset = new MediaAsset
            {
                Id = Guid.NewGuid(),
                FileUrl = fileUrl,
                MimeType = file.ContentType,
                FileName = file.FileName,
                FileExtension = fileExtension.TrimStart('.'),
                FileSizeBytes = file.Length,
                StorageProvider = "Local",
                StorageKey = filePath,
                IsPublic = false,
                UploadedByUserId = userId.Value,
                MediaTypeId = mediaType.Id
            };
            _context.Set<MediaAsset>().Add(mediaAsset);

            // Create Design
            var designTitle = Path.GetFileNameWithoutExtension(file.FileName);
            var design = new Design
            {
                Id = Guid.NewGuid(),
                UserId = userId.Value,
                Title = designTitle,
                Notes = $"Uploaded with print request {referenceNumber}",
                IsPublic = false
            };
            _context.Set<Design>().Add(design);
            designId = design.Id;

            // Create DesignFile
            var designFile = new DesignFile
            {
                Id = Guid.NewGuid(),
                DesignId = design.Id,
                MediaAssetId = mediaAsset.Id,
                Format = fileExtension.TrimStart('.').ToUpperInvariant(),
                FileSizeBytes = file.Length,
                IsPrimary = true,
                UploadedAt = DateTime.UtcNow
            };
            _context.Set<DesignFile>().Add(designFile);
        }

        // Create the Print3dServiceRequest
        var request = new Print3dServiceRequest
        {
            Id = Guid.NewGuid(),
            ReferenceNumber = referenceNumber,
            UserId = userId,
            MaterialId = materialGuid,
            MaterialColorId = materialColorGuid,
            ProfileId = profileGuid,
            DesignId = designId,
            FilePath = filePath,
            FileName = file.FileName,
            FileSizeBytes = file.Length,
            CustomerName = user?.FullName,
            CustomerEmail = user?.Email,
            CustomerNotes = comments,
            Status = Print3dServiceRequestStatus.Pending
        };

        _context.Set<Print3dServiceRequest>().Add(request);
        await _context.SaveChangesAsync(CancellationToken.None);

        try
        {
            var frontendBase = _configuration["App:FrontendBaseUrl"]?.TrimEnd('/') ?? "http://localhost:4200";
            var viewUrl = $"{frontendBase}/account/requests?requestId={request.Id}&type=print3d";
            await _emailService.SendRequestConfirmationAsync(
                request.CustomerEmail,
                request.CustomerName,
                referenceNumber,
                "3D Print Request",
                viewUrl,
                CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send confirmation email for 3D Print request {ReferenceNumber}", referenceNumber);
        }

        // Return response
        return CreatedAtAction(nameof(GetRequestById), new { id = request.Id }, new Print3dRequestDto
        {
            Id = request.Id,
            ReferenceNumber = request.ReferenceNumber,
            Status = request.Status.ToString(),
            MaterialId = request.MaterialId,
            MaterialName = material.NameEn,
            ProfileId = request.ProfileId,
            ProfileName = profile?.NameEn,
            FileName = request.FileName,
            FileSizeBytes = request.FileSizeBytes,
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            CustomerNotes = request.CustomerNotes,
            CreatedAt = request.CreatedAt
        });
    }

    /// <summary>
    /// Get all 3D print requests (admin gets all, users get their own)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<Print3dRequestListDto>> GetRequests(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null)
    {
        var query = _context.Set<Print3dServiceRequest>()
            .Include(r => r.Material)
            .Include(r => r.Profile)
            .Include(r => r.User)
            .AsQueryable();

        // Check if user is admin
        var isAdmin = User.IsInRole("Admin") || User.IsInRole("Manager");
        
        if (!isAdmin)
        {
            // Regular users only see their own requests
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
            {
                query = query.Where(r => r.UserId == userId);
            }
            else
            {
                return Ok(new Print3dRequestListDto { Items = new List<Print3dRequestDto>(), TotalCount = 0, Page = page, PageSize = pageSize });
            }
        }

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<Print3dServiceRequestStatus>(status, out var parsedStatus))
        {
            query = query.Where(r => r.Status == parsedStatus);
        }

        var totalCount = await query.CountAsync();
        var requests = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new Print3dRequestDto
            {
                Id = r.Id,
                ReferenceNumber = r.ReferenceNumber,
                Status = r.Status.ToString(),
                MaterialId = r.MaterialId,
                MaterialName = r.Material.NameEn,
                ProfileId = r.ProfileId,
                ProfileName = r.Profile != null ? r.Profile.NameEn : null,
                FileName = r.FileName,
                FileSizeBytes = r.FileSizeBytes,
                CustomerName = r.CustomerName ?? (r.User != null ? r.User.FullName : null),
                CustomerEmail = r.CustomerEmail ?? (r.User != null ? r.User.Email : null),
                CustomerNotes = r.CustomerNotes,
                EstimatedPrice = r.EstimatedPrice,
                FinalPrice = r.FinalPrice,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return Ok(new Print3dRequestListDto
        {
            Items = requests,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    /// <summary>
    /// Get a specific 3D print request by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Print3dRequestDto>> GetRequestById(Guid id)
    {
        var request = await _context.Set<Print3dServiceRequest>()
            .Include(r => r.Material)
            .Include(r => r.Profile)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null)
        {
            return NotFound();
        }

        // Check access: admin can see all, users can only see their own
        var isAdmin = User.IsInRole("Admin") || User.IsInRole("Manager");
        if (!isAdmin)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId) || request.UserId != userId)
            {
                return Forbid();
            }
        }

        return Ok(new Print3dRequestDto
        {
            Id = request.Id,
            ReferenceNumber = request.ReferenceNumber,
            Status = request.Status.ToString(),
            MaterialId = request.MaterialId,
            MaterialName = request.Material?.NameEn,
            ProfileId = request.ProfileId,
            ProfileName = request.Profile?.NameEn,
            FileName = request.FileName,
            FileSizeBytes = request.FileSizeBytes,
            CustomerName = request.CustomerName ?? request.User?.FullName,
            CustomerEmail = request.CustomerEmail ?? request.User?.Email,
            CustomerNotes = request.CustomerNotes,
            AdminNotes = request.AdminNotes,
            EstimatedPrice = request.EstimatedPrice,
            FinalPrice = request.FinalPrice,
            CreatedAt = request.CreatedAt,
            ReviewedAt = request.ReviewedAt,
            ApprovedAt = request.ApprovedAt,
            CompletedAt = request.CompletedAt
        });
    }

    /// <summary>
    /// Download the STL file for a request
    /// </summary>
    [HttpGet("{id}/file")]
    public async Task<IActionResult> DownloadFile(Guid id)
    {
        var request = await _context.Set<Print3dServiceRequest>().FindAsync(id);
        if (request == null)
        {
            return NotFound();
        }

        // Check access
        var isAdmin = User.IsInRole("Admin") || User.IsInRole("Manager");
        if (!isAdmin)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId) || request.UserId != userId)
            {
                return Forbid();
            }
        }

        var fileStream = await _fileStorageService.GetFileAsync(request.FilePath);
        if (fileStream == null)
        {
            return NotFound("File not found");
        }

        return File(fileStream, "application/octet-stream", request.FileName);
    }

    /// <summary>
    /// Update request status (admin only)
    /// </summary>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<Print3dRequestDto>> UpdateRequestStatus(Guid id, [FromBody] UpdatePrint3dRequestStatusDto dto)
    {
        var request = await _context.Set<Print3dServiceRequest>()
            .Include(r => r.Material)
            .Include(r => r.Profile)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null)
        {
            return NotFound();
        }

        if (!Enum.TryParse<Print3dServiceRequestStatus>(dto.Status, out var newStatus))
        {
            return BadRequest("Invalid status");
        }

        var previousStatus = request.Status;
        request.Status = newStatus;
        request.UpdatedAt = DateTime.UtcNow;

        // Set timestamps based on status transitions
        if (newStatus == Print3dServiceRequestStatus.UnderReview && !request.ReviewedAt.HasValue)
        {
            request.ReviewedAt = DateTime.UtcNow;
        }
        if (newStatus == Print3dServiceRequestStatus.Approved && !request.ApprovedAt.HasValue)
        {
            request.ApprovedAt = DateTime.UtcNow;
        }
        if (newStatus == Print3dServiceRequestStatus.Completed && !request.CompletedAt.HasValue)
        {
            request.CompletedAt = DateTime.UtcNow;
        }

        if (!string.IsNullOrEmpty(dto.Notes))
        {
            request.AdminNotes = dto.Notes;
        }

        if (dto.FinalPrice.HasValue)
        {
            request.FinalPrice = dto.FinalPrice.Value;
        }

        await _context.SaveChangesAsync(CancellationToken.None);

        if (newStatus == Print3dServiceRequestStatus.Cancelled &&
            previousStatus != Print3dServiceRequestStatus.Cancelled)
        {
            var toEmail = request.CustomerEmail ?? request.User?.Email;
            var custName = request.CustomerName ?? request.User?.FullName;
            var baseUrl = _configuration["App:FrontendBaseUrl"]?.TrimEnd('/') ?? "http://localhost:4200";
            var viewUrl = $"{baseUrl}/account/requests?requestId={request.Id}&type=print3d";
            await _emailService.SendRequestCancelledAsync(
                toEmail,
                custName,
                request.ReferenceNumber,
                "3D print request",
                viewUrl,
                dto.Notes,
                CancellationToken.None);
        }

        return Ok(new Print3dRequestDto
        {
            Id = request.Id,
            ReferenceNumber = request.ReferenceNumber,
            Status = request.Status.ToString(),
            MaterialId = request.MaterialId,
            MaterialName = request.Material?.NameEn,
            ProfileId = request.ProfileId,
            ProfileName = request.Profile?.NameEn,
            FileName = request.FileName,
            FileSizeBytes = request.FileSizeBytes,
            CustomerName = request.CustomerName ?? request.User?.FullName,
            CustomerEmail = request.CustomerEmail ?? request.User?.Email,
            CustomerNotes = request.CustomerNotes,
            AdminNotes = request.AdminNotes,
            EstimatedPrice = request.EstimatedPrice,
            FinalPrice = request.FinalPrice,
            CreatedAt = request.CreatedAt
        });
    }
}

// DTOs for the 3D requests endpoint
public class Print3dRequestDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid? MaterialId { get; set; }
    public string? MaterialName { get; set; }
    public Guid? ProfileId { get; set; }
    public string? ProfileName { get; set; }
    public string? FileName { get; set; }
    public long FileSizeBytes { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerNotes { get; set; }
    public string? AdminNotes { get; set; }
    public decimal? EstimatedPrice { get; set; }
    public decimal? FinalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class Print3dRequestListDto
{
    public List<Print3dRequestDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class UpdatePrint3dRequestStatusDto
{
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public decimal? FinalPrice { get; set; }
}
