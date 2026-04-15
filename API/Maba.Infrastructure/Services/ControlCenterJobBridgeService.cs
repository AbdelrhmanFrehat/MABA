using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.ControlCenterJobs;
using Maba.Application.Common.Interfaces;
using Maba.Domain.ControlCenter;

namespace Maba.Infrastructure.Services;

public class ControlCenterJobBridgeService : IControlCenterJobBridgeService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IApplicationDbContext _context;

    public ControlCenterJobBridgeService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task EnsureJobAsync(ControlCenterJobBridgeDefinition definition, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(definition.SourceType))
        {
            throw new InvalidOperationException("SourceType is required for a control-center job.");
        }

        if (definition.SourceId == Guid.Empty)
        {
            throw new InvalidOperationException("SourceId is required for a control-center job.");
        }

        if (string.IsNullOrWhiteSpace(definition.Title))
        {
            throw new InvalidOperationException("Title is required for a control-center job.");
        }

        if (string.IsNullOrWhiteSpace(definition.MachineType))
        {
            throw new InvalidOperationException("MachineType is required for a control-center job.");
        }

        var jobs = _context.Set<CcJob>();
        var job = await jobs.FirstOrDefaultAsync(
            x => x.SourceType == definition.SourceType && x.SourceId == definition.SourceId,
            cancellationToken);

        if (job == null && !definition.CreateIfMissing)
        {
            return;
        }

        if (job == null)
        {
            job = new CcJob
            {
                Id = Guid.NewGuid(),
                SourceType = definition.SourceType,
                SourceId = definition.SourceId,
                JobReference = BuildJobReference(definition),
                CreatedAt = DateTime.UtcNow
            };

            jobs.Add(job);
        }

        job.SourceReference = definition.SourceReference;
        job.Title = definition.Title.Trim();
        job.Description = Normalize(definition.Description);
        job.CustomerName = Normalize(definition.CustomerName);
        job.MachineType = definition.MachineType.Trim();
        job.ModuleId = definition.ModuleId;
        job.Status = definition.Status;
        job.Priority = Normalize(definition.Priority);
        job.DeviceId = definition.AssignedDeviceId;
        job.AttachmentsJson = definition.Attachments.Count == 0
            ? null
            : JsonSerializer.Serialize(definition.Attachments, JsonOptions);
        job.PayloadJson = Normalize(definition.PayloadJson);
        job.ParametersJson = Normalize(definition.ParametersJson);

        if (definition.Status == CcJobStatus.InProgress && job.StartedAt == null)
        {
            job.StartedAt = DateTime.UtcNow;
        }

        if (definition.Status == CcJobStatus.Completed)
        {
            job.CompletedAt ??= DateTime.UtcNow;
            job.Progress ??= 100;
        }
        else if (definition.Status is CcJobStatus.Failed or CcJobStatus.Cancelled)
        {
            job.CompletedAt ??= DateTime.UtcNow;
        }
        else
        {
            job.CompletedAt = null;
        }

        job.UpdatedAt = DateTime.UtcNow;
    }

    private static string BuildJobReference(ControlCenterJobBridgeDefinition definition)
    {
        if (!string.IsNullOrWhiteSpace(definition.JobReference))
        {
            return definition.JobReference.Trim();
        }

        if (!string.IsNullOrWhiteSpace(definition.SourceReference))
        {
            return $"JOB-{definition.SourceReference.Trim()}";
        }

        return $"JOB-{DateTime.UtcNow:yyyyMMdd}-{definition.SourceId.ToString("N")[..8].ToUpperInvariant()}";
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
