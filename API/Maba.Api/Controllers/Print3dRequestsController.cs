using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Maba.Application.Common.Interfaces;
using Maba.Application.Common.ServiceRequests;
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
    private readonly IPricingService _pricingService;

    public Print3dRequestsController(
        IMediator mediator,
        IFileStorageService fileStorageService,
        IApplicationDbContext context,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<Print3dRequestsController> logger,
        IPricingService pricingService)
    {
        _mediator = mediator;
        _fileStorageService = fileStorageService;
        _context = context;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
        _pricingService = pricingService;
    }

    private static string? FormatUsedSpoolLabel(FilamentSpool? spool)
    {
        if (spool == null)
        {
            return null;
        }

        var material = spool.Material?.NameEn ?? "?";
        var spoolName = !string.IsNullOrWhiteSpace(spool.Name) ? spool.Name.Trim() : "—";
        return $"{material} — {spoolName} ({spool.RemainingWeightGrams}g)";
    }

    private static Print3dRequestDto MapToDto(Print3dServiceRequest r) => new()
    {
        Id = r.Id,
        ReferenceNumber = r.ReferenceNumber,
        Status = r.Status.ToString(),
        WorkflowStatus = ServiceRequestWorkflowMapper.FromPrint3d(r.Status).ToString(),
        MaterialId = r.MaterialId,
        MaterialName = r.Material?.NameEn,
        ProfileId = r.ProfileId,
        ProfileName = r.Profile?.NameEn,
        FileName = r.FileName,
        FileSizeBytes = r.FileSizeBytes,
        CustomerName = r.CustomerName ?? r.User?.FullName,
        CustomerEmail = r.CustomerEmail ?? r.User?.Email,
        CustomerNotes = r.CustomerNotes,
        AdminNotes = r.AdminNotes,
        EstimatedPrice = r.EstimatedPrice,
        FinalPrice = r.FinalPrice,
        CreatedAt = r.CreatedAt,
        ReviewedAt = r.ReviewedAt,
        ApprovedAt = r.ApprovedAt,
        CompletedAt = r.CompletedAt,
        UsedSpoolId = r.UsedSpoolId,
        EstimatedFilamentGrams = r.EstimatedFilamentGrams,
        ActualFilamentGrams = r.ActualFilamentGrams,
        UsedSpoolName = FormatUsedSpoolLabel(r.UsedSpool),
        IsFilamentDeducted = r.IsFilamentDeducted,
        EstimatedPrintTimeHours = r.EstimatedPrintTimeHours,
        SuggestedPrice = r.SuggestedPrice,
        MaterialPricePerGram = r.Material?.PricePerGram,
        ProfilePriceMultiplier = r.Profile?.PriceMultiplier
    };

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
            WorkflowStatus = ServiceRequestWorkflowMapper.FromPrint3d(request.Status).ToString(),
            MaterialId = request.MaterialId,
            MaterialName = material.NameEn,
            ProfileId = request.ProfileId,
            ProfileName = profile?.NameEn,
            FileName = request.FileName,
            FileSizeBytes = request.FileSizeBytes,
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            CustomerNotes = request.CustomerNotes,
            CreatedAt = request.CreatedAt,
            UsedSpoolId = null,
            EstimatedFilamentGrams = null,
            ActualFilamentGrams = null,
            UsedSpoolName = null,
            IsFilamentDeducted = false,
            EstimatedPrintTimeHours = null,
            SuggestedPrice = null,
            MaterialPricePerGram = material.PricePerGram,
            ProfilePriceMultiplier = profile?.PriceMultiplier
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
            .Include(r => r.UsedSpool!)
                .ThenInclude(s => s.Material)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = requests.Select(MapToDto).ToList();

        return Ok(new Print3dRequestListDto
        {
            Items = items,
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
            .Include(r => r.UsedSpool!)
                .ThenInclude(s => s.Material)
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

        return Ok(MapToDto(request));
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

        request.EstimatedPrintTimeHours = dto.EstimatedPrintTimeHours;
        request.SuggestedPrice = dto.SuggestedPrice;

        if (!request.IsFilamentDeducted)
        {
            if (dto.EstimatedFilamentGrams.HasValue && dto.EstimatedFilamentGrams.Value < 0)
            {
                return BadRequest("EstimatedFilamentGrams must be >= 0");
            }

            if (dto.UsedSpoolId.HasValue)
            {
                var spoolExists = await _context.Set<FilamentSpool>()
                    .AnyAsync(s => s.Id == dto.UsedSpoolId.Value, CancellationToken.None);
                if (!spoolExists)
                {
                    return BadRequest("Filament spool not found.");
                }
            }

            request.UsedSpoolId = dto.UsedSpoolId;
            request.EstimatedFilamentGrams = dto.EstimatedFilamentGrams;
        }

        // Controlled filament deduction: once per request, only on transition into Approved (backend only).
        if (newStatus == Print3dServiceRequestStatus.Approved
            && previousStatus != Print3dServiceRequestStatus.Approved
            && !request.IsFilamentDeducted
            && request.UsedSpoolId.HasValue
            && request.EstimatedFilamentGrams.HasValue)
        {
            var spool = await _context.Set<FilamentSpool>()
                .Include(s => s.Material)
                .FirstOrDefaultAsync(s => s.Id == request.UsedSpoolId!.Value, CancellationToken.None);
            if (spool == null)
            {
                return BadRequest("Filament spool not found.");
            }

            var usedGrams = request.EstimatedFilamentGrams.Value;
            spool.RemainingWeightGrams -= usedGrams;
            request.IsFilamentDeducted = true;

            var spoolLogName = FormatUsedSpoolLabel(spool) ?? spool.Id.ToString();
            _logger.LogInformation(
                "[3D PRINT]" + Environment.NewLine
                + "Request: {ReferenceNumber}" + Environment.NewLine
                + "Spool: {SpoolName}" + Environment.NewLine
                + "Used: {UsedGrams}g" + Environment.NewLine
                + "Remaining: {RemainingGrams}g",
                request.ReferenceNumber,
                spoolLogName,
                usedGrams,
                spool.RemainingWeightGrams);
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

        var refreshed = await _context.Set<Print3dServiceRequest>()
            .AsNoTracking()
            .Include(r => r.Material)
            .Include(r => r.Profile)
            .Include(r => r.User)
            .Include(r => r.UsedSpool!)
                .ThenInclude(s => s.Material)
            .FirstAsync(r => r.Id == id, CancellationToken.None);

        return Ok(MapToDto(refreshed));
    }

    /// <summary>
    /// Compute a suggested price from material, profile, and time (does not persist — save via PUT /status).
    /// </summary>
    [HttpPost("{id}/pricing-suggestion")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<PricingSuggestionResponseDto>> GetPricingSuggestion(Guid id, [FromBody] PricingSuggestionRequestDto dto)
    {
        var request = await _context.Set<Print3dServiceRequest>()
            .Include(r => r.Material)
            .Include(r => r.Profile)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null)
        {
            return NotFound();
        }

        if (dto.EstimatedPrintTimeHours <= 0)
        {
            return BadRequest("EstimatedPrintTimeHours must be greater than zero.");
        }

        var gramsInt = dto.EstimatedFilamentGrams ?? request.EstimatedFilamentGrams;
        if (!gramsInt.HasValue || gramsInt.Value < 0)
        {
            return BadRequest("Estimated filament grams are required (enter and save grams, or pass estimatedFilamentGrams in the request body).");
        }

        var grams = (decimal)gramsInt.Value;
        var costPerGram = request.Material?.PricePerGram ?? 0m;
        if (costPerGram < 0)
        {
            return BadRequest("Invalid material price per gram.");
        }

        var profileId = dto.ProfileId ?? request.ProfileId;
        decimal qualityMultiplier = 1m;
        if (profileId.HasValue)
        {
            var profile = await _context.Set<PrintQualityProfile>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == profileId.Value, CancellationToken.None);
            if (profile != null)
            {
                qualityMultiplier = profile.PriceMultiplier;
            }
        }
        else if (request.Profile != null)
        {
            qualityMultiplier = request.Profile.PriceMultiplier;
        }

        var defaultHourly = _configuration.GetValue<decimal>("App:Print3dPricing:DefaultHourlyRate", 15m);
        var hourlyRate = dto.HourlyRate ?? defaultHourly;
        if (hourlyRate < 0)
        {
            return BadRequest("Hourly rate must be non-negative.");
        }

        var defaultProfitMargin = _configuration.GetValue<decimal>("App:Print3dPricing:DefaultProfitMargin", 1.5m);
        var defaultMinimumPrice = _configuration.GetValue<decimal>("App:Print3dPricing:DefaultMinimumPrice", 10m);
        var profitMargin = dto.ProfitMargin ?? defaultProfitMargin;
        var minimumPrice = dto.MinimumPrice ?? defaultMinimumPrice;
        if (profitMargin <= 0)
        {
            return BadRequest("Profit margin must be greater than zero.");
        }

        if (minimumPrice < 0)
        {
            return BadRequest("Minimum price must be non-negative.");
        }

        decimal? roundToNearest;
        if (dto.RoundToNearest.HasValue)
        {
            if (dto.RoundToNearest.Value < 0)
            {
                return BadRequest("Round step must be non-negative.");
            }

            roundToNearest = dto.RoundToNearest.Value == 0 ? null : dto.RoundToNearest.Value;
        }
        else
        {
            var cfgRound = _configuration.GetValue<decimal?>("App:Print3dPricing:RoundToNearest", 5m);
            roundToNearest = cfgRound is > 0 ? cfgRound : null;
        }

        var printHours = dto.EstimatedPrintTimeHours;
        var adv = _pricingService.CalculateAdvancedPrice(
            grams,
            costPerGram,
            printHours,
            hourlyRate,
            qualityMultiplier,
            profitMargin,
            minimumPrice,
            roundToNearest);

        return Ok(new PricingSuggestionResponseDto
        {
            SuggestedPrice = adv.FinalSuggested,
            MaterialCost = adv.MaterialCost,
            MachineCost = adv.MachineCost,
            BaseCost = adv.BaseCost,
            AdjustedCost = adv.AdjustedCost,
            AfterMargin = adv.AfterMargin,
            AfterMinimum = adv.AfterMinimum,
            MinimumApplied = adv.MinimumApplied,
            RoundingApplied = adv.RoundingApplied,
            RoundStep = adv.RoundStep,
            Grams = grams,
            CostPerGram = costPerGram,
            PrintTimeHours = printHours,
            HourlyRate = hourlyRate,
            QualityMultiplier = qualityMultiplier,
            ProfitMargin = profitMargin,
            MinimumPrice = minimumPrice,
            DefaultHourlyRate = defaultHourly,
            DefaultProfitMargin = defaultProfitMargin,
            DefaultMinimumPrice = defaultMinimumPrice
        });
    }
}

// DTOs for the 3D requests endpoint
public class Print3dRequestDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string WorkflowStatus { get; set; } = string.Empty;
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
    public Guid? UsedSpoolId { get; set; }
    public int? EstimatedFilamentGrams { get; set; }
    public int? ActualFilamentGrams { get; set; }
    /// <summary>Display label for the linked spool, e.g. "PLA Black — S1 (950g)".</summary>
    public string? UsedSpoolName { get; set; }
    public bool IsFilamentDeducted { get; set; }
    public decimal? EstimatedPrintTimeHours { get; set; }
    public decimal? SuggestedPrice { get; set; }
    /// <summary>From material — for pricing assistant display.</summary>
    public decimal? MaterialPricePerGram { get; set; }
    /// <summary>From quality profile — applied to base (material + machine) cost.</summary>
    public decimal? ProfilePriceMultiplier { get; set; }
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
    public Guid? UsedSpoolId { get; set; }
    public int? EstimatedFilamentGrams { get; set; }
    public decimal? EstimatedPrintTimeHours { get; set; }
    public decimal? SuggestedPrice { get; set; }
}

public class PricingSuggestionRequestDto
{
    public decimal EstimatedPrintTimeHours { get; set; }
    public decimal? HourlyRate { get; set; }
    public Guid? ProfileId { get; set; }
    /// <summary>Optional override when form grams differ from saved request.</summary>
    public int? EstimatedFilamentGrams { get; set; }
    public decimal? ProfitMargin { get; set; }
    public decimal? MinimumPrice { get; set; }
    /// <summary>Optional rounding step (e.g. 0.5, 1, 5). Use 0 to disable rounding.</summary>
    public decimal? RoundToNearest { get; set; }
}

public class PricingSuggestionResponseDto
{
    public decimal SuggestedPrice { get; set; }
    public decimal MaterialCost { get; set; }
    public decimal MachineCost { get; set; }
    public decimal BaseCost { get; set; }
    public decimal AdjustedCost { get; set; }
    public decimal AfterMargin { get; set; }
    public decimal AfterMinimum { get; set; }
    public bool MinimumApplied { get; set; }
    public bool RoundingApplied { get; set; }
    public decimal? RoundStep { get; set; }
    public decimal Grams { get; set; }
    public decimal CostPerGram { get; set; }
    public decimal PrintTimeHours { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal QualityMultiplier { get; set; }
    public decimal ProfitMargin { get; set; }
    public decimal MinimumPrice { get; set; }
    public decimal DefaultHourlyRate { get; set; }
    public decimal DefaultProfitMargin { get; set; }
    public decimal DefaultMinimumPrice { get; set; }
}
