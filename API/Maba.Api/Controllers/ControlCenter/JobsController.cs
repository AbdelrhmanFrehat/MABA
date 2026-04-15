using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Domain.ControlCenter;

namespace Maba.Api.Controllers.ControlCenter;

[ApiController]
[Route("api/v1/control-center/jobs")]
[AllowAnonymous]
public class JobsController : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IApplicationDbContext _context;

    public JobsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ControlCenterJobListItemDto>>> GetJobs(
        [FromQuery] string? status = null,
        [FromQuery] string? machineType = null,
        CancellationToken cancellationToken = default)
    {
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
        var job = await _context.Set<CcJob>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (job == null)
        {
            return NotFound();
        }

        return Ok(new ControlCenterJobDetailDto
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
        });
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateControlCenterJobStatusDto dto, CancellationToken cancellationToken = default)
    {
        var job = await _context.Set<CcJob>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (job == null)
        {
            return NotFound();
        }

        if (!Enum.TryParse<CcJobStatus>(dto.Status, true, out var parsedStatus))
        {
            return BadRequest(new { error = "Invalid job status." });
        }

        job.Status = parsedStatus;
        job.UpdatedAt = DateTime.UtcNow;

        if (parsedStatus == CcJobStatus.InProgress && job.StartedAt == null)
        {
            job.StartedAt = DateTime.UtcNow;
        }

        if (parsedStatus is CcJobStatus.Completed or CcJobStatus.Failed or CcJobStatus.Cancelled)
        {
            job.CompletedAt ??= DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
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
