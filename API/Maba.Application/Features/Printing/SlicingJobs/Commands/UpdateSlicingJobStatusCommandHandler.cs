using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Printing.SlicingJobs.Commands;
using Maba.Application.Features.Printing.DTOs;
using Maba.Domain.Printing;

namespace Maba.Application.Features.Printing.SlicingJobs.Handlers;

public class UpdateSlicingJobStatusCommandHandler : IRequestHandler<UpdateSlicingJobStatusCommand, SlicingJobDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateSlicingJobStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SlicingJobDto> Handle(UpdateSlicingJobStatusCommand request, CancellationToken cancellationToken)
    {
        var slicingJob = await _context.Set<SlicingJob>()
            .Include(sj => sj.SlicingJobStatus)
            .FirstOrDefaultAsync(sj => sj.Id == request.SlicingJobId, cancellationToken);

        if (slicingJob == null)
        {
            throw new KeyNotFoundException("Slicing job not found.");
        }

        slicingJob.SlicingJobStatusId = request.SlicingJobStatusId;
        slicingJob.UpdatedAt = DateTime.UtcNow;

        if (request.ErrorMessage != null)
        {
            slicingJob.ErrorMessage = request.ErrorMessage;
        }

        if (request.OutputFileUrl != null)
        {
            slicingJob.OutputFileUrl = request.OutputFileUrl;
        }

        if (request.EstimatedCost.HasValue)
        {
            slicingJob.EstimatedCost = request.EstimatedCost.Value;
        }

        // If status is completed, set CompletedAt
        var completedStatus = await _context.Set<SlicingJobStatus>()
            .FirstOrDefaultAsync(s => s.Key == "Completed", cancellationToken);

        if (completedStatus != null && slicingJob.SlicingJobStatusId == completedStatus.Id)
        {
            slicingJob.CompletedAt = DateTime.UtcNow;
        }

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

