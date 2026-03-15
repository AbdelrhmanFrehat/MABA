using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Printing.SlicingJobs.Commands;
using Maba.Application.Features.Printing.DTOs;
using Maba.Domain.Printing;

namespace Maba.Application.Features.Printing.SlicingJobs.Handlers;

public class CancelSlicingJobCommandHandler : IRequestHandler<CancelSlicingJobCommand, SlicingJobDto>
{
    private readonly IApplicationDbContext _context;

    public CancelSlicingJobCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SlicingJobDto> Handle(CancelSlicingJobCommand request, CancellationToken cancellationToken)
    {
        var slicingJob = await _context.Set<SlicingJob>()
            .Include(sj => sj.SlicingJobStatus)
            .Include(sj => sj.PrintJobs)
            .FirstOrDefaultAsync(sj => sj.Id == request.SlicingJobId, cancellationToken);

        if (slicingJob == null)
        {
            throw new KeyNotFoundException("Slicing job not found.");
        }

        // Check if there are active print jobs
        var activePrintJobStatuses = await _context.Set<PrintJobStatus>()
            .Where(s => s.Key == "Queued" || s.Key == "Printing" || s.Key == "Paused")
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        if (slicingJob.PrintJobs.Any(pj => activePrintJobStatuses.Contains(pj.PrintJobStatusId)))
        {
            throw new InvalidOperationException("Cannot cancel slicing job with active print jobs. Cancel print jobs first.");
        }

        var cancelledStatus = await _context.Set<SlicingJobStatus>()
            .FirstOrDefaultAsync(s => s.Key == "Cancelled", cancellationToken);

        if (cancelledStatus == null)
        {
            throw new InvalidOperationException("Cancelled status not found.");
        }

        slicingJob.SlicingJobStatusId = cancelledStatus.Id;
        slicingJob.ErrorMessage = request.Reason ?? "Cancelled by user";
        slicingJob.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new SlicingJobDto
        {
            Id = slicingJob.Id,
            DesignFileId = slicingJob.DesignFileId,
            SlicingProfileId = slicingJob.SlicingProfileId,
            SlicingJobStatusId = slicingJob.SlicingJobStatusId,
            SlicingJobStatusKey = slicingJob.SlicingJobStatus.Key,
            EstimatedTimeMin = slicingJob.EstimatedTimeMin,
            EstimatedMaterialGrams = slicingJob.EstimatedMaterialGrams,
            PriceEstimate = slicingJob.PriceEstimate,
            EstimatedCost = slicingJob.EstimatedCost,
            ErrorMessage = slicingJob.ErrorMessage,
            OutputFileUrl = slicingJob.OutputFileUrl,
            CompletedAt = slicingJob.CompletedAt,
            CreatedAt = slicingJob.CreatedAt,
            UpdatedAt = slicingJob.UpdatedAt
        };
    }
}

