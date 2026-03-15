using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Printing.SlicingJobs.Commands;
using Maba.Application.Features.Printing.DTOs;
using Maba.Domain.Printing;

namespace Maba.Application.Features.Printing.SlicingJobs.Handlers;

public class CreateSlicingJobCommandHandler : IRequestHandler<CreateSlicingJobCommand, SlicingJobDto>
{
    private readonly IApplicationDbContext _context;

    public CreateSlicingJobCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SlicingJobDto> Handle(CreateSlicingJobCommand request, CancellationToken cancellationToken)
    {
        var designFile = await _context.Set<DesignFile>()
            .FirstOrDefaultAsync(df => df.Id == request.DesignFileId, cancellationToken);

        if (designFile == null)
        {
            throw new KeyNotFoundException("Design file not found");
        }

        var profile = await _context.Set<SlicingProfile>()
            .FirstOrDefaultAsync(sp => sp.Id == request.SlicingProfileId, cancellationToken);

        if (profile == null)
        {
            throw new KeyNotFoundException("Slicing profile not found");
        }

        var pendingStatus = await _context.Set<SlicingJobStatus>()
            .FirstOrDefaultAsync(s => s.Key == "Pending", cancellationToken);

        if (pendingStatus == null)
        {
            throw new KeyNotFoundException("Pending slicing job status not found");
        }

        var slicingJob = new SlicingJob
        {
            Id = Guid.NewGuid(),
            DesignFileId = request.DesignFileId,
            SlicingProfileId = request.SlicingProfileId,
            SlicingJobStatusId = pendingStatus.Id
        };

        _context.Set<SlicingJob>().Add(slicingJob);
        await _context.SaveChangesAsync(cancellationToken);

        return new SlicingJobDto
        {
            Id = slicingJob.Id,
            DesignFileId = slicingJob.DesignFileId,
            SlicingProfileId = slicingJob.SlicingProfileId,
            SlicingJobStatusId = slicingJob.SlicingJobStatusId,
            SlicingJobStatusKey = pendingStatus.Key,
            EstimatedTimeMin = slicingJob.EstimatedTimeMin,
            EstimatedMaterialGrams = slicingJob.EstimatedMaterialGrams,
            PriceEstimate = slicingJob.PriceEstimate,
            CompletedAt = slicingJob.CompletedAt,
            CreatedAt = slicingJob.CreatedAt,
            UpdatedAt = slicingJob.UpdatedAt
        };
    }
}

