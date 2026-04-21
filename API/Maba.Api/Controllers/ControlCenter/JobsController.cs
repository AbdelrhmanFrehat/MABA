using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.ControlCenterJobs;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Cnc;
using Maba.Domain.ControlCenter;
using Maba.Domain.Laser;
using Maba.Domain.Printing;

namespace Maba.Api.Controllers.ControlCenter;

[ApiController]
[Route("api/v1/control-center/jobs")]
[AllowAnonymous]
public class JobsController : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IApplicationDbContext _context;
    private readonly IControlCenterJobBridgeService _jobBridgeService;

    public JobsController(IApplicationDbContext context, IControlCenterJobBridgeService jobBridgeService)
    {
        _context = context;
        _jobBridgeService = jobBridgeService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ControlCenterJobListItemDto>>> GetJobs(
        [FromQuery] string? status = null,
        [FromQuery] string? machineType = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureBackfilledJobsAsync(cancellationToken);

        var query = _context.Set<CcJob>()
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<CcJobStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(x => x.Status == parsedStatus);
        }

        if (!string.IsNullOrWhiteSpace(machineType))
        {
            var normalizedMachineType = machineType.Trim();
            query = query.Where(x => x.MachineType == normalizedMachineType);
        }

        var jobs = await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ControlCenterJobListItemDto
            {
                Id = x.Id,
                JobReference = x.JobReference,
                SourceType = x.SourceType,
                SourceId = x.SourceId,
                SourceReference = x.SourceReference,
                Title = x.Title,
                Description = x.Description,
                CustomerName = x.CustomerName,
                MachineType = x.MachineType,
                ModuleId = x.ModuleId,
                Status = x.Status.ToString(),
                Priority = x.Priority,
                AssignedDeviceId = x.DeviceId,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(jobs);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ControlCenterJobDetailDto>> GetJob(Guid id, CancellationToken cancellationToken = default)
    {
        await EnsureBackfilledJobsAsync(cancellationToken);

        var job = await _context.Set<CcJob>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (job == null)
        {
            return NotFound();
        }

        return Ok(MapDetail(job));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateControlCenterJobStatusDto dto, CancellationToken cancellationToken = default)
    {
        return BadRequest(new
        {
            error = "Direct status edits are not allowed. Use job workflow actions instead."
        });
    }

    [HttpPost("{id:guid}/actions/{action}")]
    public async Task<ActionResult<ControlCenterJobDetailDto>> RunAction(Guid id, string action, CancellationToken cancellationToken = default)
        => await ExecuteActionAsync(id, action, cancellationToken);

    [HttpPost("{id:guid}/mark-ready")]
    public async Task<ActionResult<ControlCenterJobDetailDto>> MarkReady(Guid id, CancellationToken cancellationToken = default)
        => await ExecuteActionAsync(id, "mark-ready", cancellationToken);

    [HttpPost("{id:guid}/start")]
    public async Task<ActionResult<ControlCenterJobDetailDto>> Start(Guid id, CancellationToken cancellationToken = default)
        => await ExecuteActionAsync(id, "start", cancellationToken);

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<ControlCenterJobDetailDto>> Complete(Guid id, CancellationToken cancellationToken = default)
        => await ExecuteActionAsync(id, "complete", cancellationToken);

    [HttpPost("{id:guid}/fail")]
    public async Task<ActionResult<ControlCenterJobDetailDto>> Fail(Guid id, CancellationToken cancellationToken = default)
        => await ExecuteActionAsync(id, "fail", cancellationToken);

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ControlCenterJobDetailDto>> Cancel(Guid id, CancellationToken cancellationToken = default)
        => await ExecuteActionAsync(id, "cancel", cancellationToken);

    private async Task<ActionResult<ControlCenterJobDetailDto>> ExecuteActionAsync(Guid id, string action, CancellationToken cancellationToken)
    {
        var job = await _context.Set<CcJob>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (job == null)
        {
            return NotFound();
        }

        var normalizedAction = action.Trim().ToLowerInvariant();
        var now = DateTime.UtcNow;

        switch (normalizedAction)
        {
            case "mark-ready" or "ready" or "approve" when job.Status == CcJobStatus.Pending:
                job.Status = CcJobStatus.Ready;
                break;

            case "start" when job.Status == CcJobStatus.Ready:
                job.Status = CcJobStatus.InProgress;
                job.StartedAt ??= now;
                job.CompletedAt = null;
                break;

            case "complete" when job.Status == CcJobStatus.InProgress:
                job.Status = CcJobStatus.Completed;
                job.StartedAt ??= now;
                job.CompletedAt ??= now;
                break;

            case "fail" when job.Status == CcJobStatus.InProgress:
                job.Status = CcJobStatus.Failed;
                job.StartedAt ??= now;
                job.CompletedAt ??= now;
                break;

            case "cancel" when job.Status != CcJobStatus.Cancelled:
                job.Status = CcJobStatus.Cancelled;
                job.CompletedAt ??= now;
                break;

            default:
                return BadRequest(new
                {
                    error = $"Action '{action}' is not valid for job status '{job.Status}'."
                });
        }

        job.UpdatedAt = now;
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(MapDetail(job));
    }

    private async Task EnsureBackfilledJobsAsync(CancellationToken cancellationToken)
    {
        await BackfillPrintJobsAsync(cancellationToken);
        await BackfillCncJobsAsync(cancellationToken);
        await BackfillLaserJobsAsync(cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task BackfillPrintJobsAsync(CancellationToken cancellationToken)
    {
        var eligibleStatuses = new[]
        {
            Print3dServiceRequestStatus.Approved,
            Print3dServiceRequestStatus.Queued,
            Print3dServiceRequestStatus.Slicing,
            Print3dServiceRequestStatus.Printing,
            Print3dServiceRequestStatus.Completed,
            Print3dServiceRequestStatus.Failed
        };

        var requests = await _context.Set<Print3dServiceRequest>()
            .AsNoTracking()
            .Include(x => x.Material)
            .Include(x => x.Profile)
            .Include(x => x.User)
            .Where(x => eligibleStatuses.Contains(x.Status))
            .ToListAsync(cancellationToken);

        foreach (var request in requests)
        {
            var (jobStatus, createIfMissing) = request.Status switch
            {
                Print3dServiceRequestStatus.Approved or Print3dServiceRequestStatus.Queued => (CcJobStatus.Ready, true),
                Print3dServiceRequestStatus.Slicing or Print3dServiceRequestStatus.Printing => (CcJobStatus.InProgress, true),
                Print3dServiceRequestStatus.Completed => (CcJobStatus.Completed, true),
                Print3dServiceRequestStatus.Failed => (CcJobStatus.Failed, true),
                _ => (CcJobStatus.Pending, false)
            };

            await _jobBridgeService.EnsureJobAsync(new ControlCenterJobBridgeDefinition
            {
                SourceType = "PRINT_REQUEST",
                SourceId = request.Id,
                SourceReference = request.ReferenceNumber,
                Title = "3D print production job",
                Description = request.CustomerNotes ?? request.AdminNotes,
                CustomerName = request.CustomerName ?? request.User?.FullName,
                MachineType = "PRINTER_3D",
                Status = jobStatus,
                Priority = request.Profile?.NameEn,
                Attachments = string.IsNullOrWhiteSpace(request.FileName)
                    ? Array.Empty<ControlCenterJobFileReference>()
                    : new[]
                    {
                        new ControlCenterJobFileReference
                        {
                            FileName = request.FileName,
                            FilePath = request.FilePath,
                            Kind = "model"
                        }
                    },
                PayloadJson = JsonSerializer.Serialize(new
                {
                    Material = request.Material?.NameEn,
                    Profile = request.Profile?.NameEn,
                    request.MaterialColorId,
                    request.EstimatedFilamentGrams,
                    request.EstimatedPrintTimeHours,
                    request.SuggestedPrice,
                    request.FinalPrice,
                    request.FileSizeBytes,
                    request.CustomerNotes
                }),
                CreateIfMissing = createIfMissing
            }, cancellationToken);
        }
    }

    private async Task BackfillCncJobsAsync(CancellationToken cancellationToken)
    {
        var eligibleStatuses = new[]
        {
            CncServiceRequestStatus.Accepted,
            CncServiceRequestStatus.InProgress,
            CncServiceRequestStatus.Completed
        };

        var requests = await _context.Set<CncServiceRequest>()
            .AsNoTracking()
            .Where(x => eligibleStatuses.Contains(x.Status))
            .ToListAsync(cancellationToken);

        foreach (var request in requests)
        {
            var (jobStatus, createIfMissing) = request.Status switch
            {
                CncServiceRequestStatus.Accepted => (CcJobStatus.Ready, true),
                CncServiceRequestStatus.InProgress => (CcJobStatus.InProgress, true),
                CncServiceRequestStatus.Completed => (CcJobStatus.Completed, true),
                _ => (CcJobStatus.Pending, false)
            };

            await _jobBridgeService.EnsureJobAsync(new ControlCenterJobBridgeDefinition
            {
                SourceType = "CNC_REQUEST",
                SourceId = request.Id,
                SourceReference = request.ReferenceNumber,
                Title = $"CNC {request.ServiceMode} job",
                Description = request.ProjectDescription ?? request.DesignNotes ?? request.AdminNotes,
                CustomerName = request.CustomerName,
                MachineType = "CNC",
                Status = jobStatus,
                Priority = request.OperationType,
                Attachments = string.IsNullOrWhiteSpace(request.FileName)
                    ? Array.Empty<ControlCenterJobFileReference>()
                    : new[]
                    {
                        new ControlCenterJobFileReference
                        {
                            FileName = request.FileName!,
                            FilePath = request.FilePath,
                            Kind = "source-file"
                        }
                    },
                PayloadJson = JsonSerializer.Serialize(new
                {
                    request.ServiceMode,
                    request.OperationType,
                    request.PcbMaterial,
                    request.PcbThickness,
                    request.PcbSide,
                    request.PcbOperation,
                    request.WidthMm,
                    request.HeightMm,
                    request.ThicknessMm,
                    request.Quantity,
                    request.DepthMode,
                    request.DepthMm,
                    request.DesignSourceType,
                    request.DesignNotes,
                    request.FinalPrice,
                    request.EstimatedPrice
                }),
                CreateIfMissing = createIfMissing
            }, cancellationToken);
        }
    }

    private async Task BackfillLaserJobsAsync(CancellationToken cancellationToken)
    {
        var eligibleStatuses = new[]
        {
            LaserServiceRequestStatus.Approved,
            LaserServiceRequestStatus.InProgress,
            LaserServiceRequestStatus.Completed
        };

        var requests = await _context.Set<LaserServiceRequest>()
            .AsNoTracking()
            .Include(x => x.Material)
            .Where(x => eligibleStatuses.Contains(x.Status))
            .ToListAsync(cancellationToken);

        foreach (var request in requests)
        {
            var (jobStatus, createIfMissing) = request.Status switch
            {
                LaserServiceRequestStatus.Approved => (CcJobStatus.Ready, true),
                LaserServiceRequestStatus.InProgress => (CcJobStatus.InProgress, true),
                LaserServiceRequestStatus.Completed => (CcJobStatus.Completed, true),
                _ => (CcJobStatus.Pending, false)
            };

            await _jobBridgeService.EnsureJobAsync(new ControlCenterJobBridgeDefinition
            {
                SourceType = "LASER_REQUEST",
                SourceId = request.Id,
                SourceReference = request.ReferenceNumber,
                Title = request.OperationMode == "cut"
                    ? "Laser cutting job"
                    : "Laser engraving job",
                Description = request.CustomerNotes ?? request.AdminNotes,
                CustomerName = request.CustomerName,
                MachineType = "LASER",
                Status = jobStatus,
                Attachments = string.IsNullOrWhiteSpace(request.ImageFileName)
                    ? Array.Empty<ControlCenterJobFileReference>()
                    : new[]
                    {
                        new ControlCenterJobFileReference
                        {
                            FileName = request.ImageFileName,
                            FilePath = request.ImagePath,
                            Kind = "artwork"
                        }
                    },
                PayloadJson = JsonSerializer.Serialize(new
                {
                    request.OperationMode,
                    Material = request.Material?.NameEn,
                    request.WidthCm,
                    request.HeightCm,
                    request.QuotedPrice,
                    request.CustomerNotes
                }),
                CreateIfMissing = createIfMissing
            }, cancellationToken);
        }
    }

    private static List<ControlCenterJobAttachmentDto> DeserializeAttachments(string? attachmentsJson)
    {
        if (string.IsNullOrWhiteSpace(attachmentsJson))
        {
            return new List<ControlCenterJobAttachmentDto>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<ControlCenterJobAttachmentDto>>(attachmentsJson, JsonOptions)
                ?? new List<ControlCenterJobAttachmentDto>();
        }
        catch
        {
            return new List<ControlCenterJobAttachmentDto>();
        }
    }

    private static ControlCenterJobDetailDto MapDetail(CcJob job)
    {
        return new ControlCenterJobDetailDto
        {
            Id = job.Id,
            JobReference = job.JobReference,
            SourceType = job.SourceType,
            SourceId = job.SourceId,
            SourceReference = job.SourceReference,
            Title = job.Title,
            Description = job.Description,
            CustomerName = job.CustomerName,
            MachineType = job.MachineType,
            ModuleId = job.ModuleId,
            Status = job.Status.ToString(),
            Priority = job.Priority,
            AssignedDeviceId = job.DeviceId,
            CreatedAt = job.CreatedAt,
            UpdatedAt = job.UpdatedAt,
            StartedAt = job.StartedAt,
            CompletedAt = job.CompletedAt,
            ResultSummary = job.ResultSummary,
            Attachments = DeserializeAttachments(job.AttachmentsJson),
            PayloadJson = job.PayloadJson
        };
    }
}

public class ControlCenterJobListItemDto
{
    public Guid Id { get; set; }
    public string JobReference { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public Guid? SourceId { get; set; }
    public string? SourceReference { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CustomerName { get; set; }
    public string? MachineType { get; set; }
    public Guid? ModuleId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Priority { get; set; }
    public Guid? AssignedDeviceId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ControlCenterJobDetailDto : ControlCenterJobListItemDto
{
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ResultSummary { get; set; }
    public List<ControlCenterJobAttachmentDto> Attachments { get; set; } = new();
    public string? PayloadJson { get; set; }
}

public class ControlCenterJobAttachmentDto
{
    public string FileName { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public string? FileUrl { get; set; }
    public string? Kind { get; set; }
}

public class UpdateControlCenterJobStatusDto
{
    public string Status { get; set; } = string.Empty;
}
