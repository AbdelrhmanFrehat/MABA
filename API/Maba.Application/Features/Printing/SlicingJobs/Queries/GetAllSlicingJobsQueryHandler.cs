using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Printing.DTOs;
using Maba.Application.Features.Printing.SlicingJobs.Queries;
using Maba.Domain.Printing;

namespace Maba.Application.Features.Printing.SlicingJobs.Handlers;

public class GetAllSlicingJobsQueryHandler : IRequestHandler<GetAllSlicingJobsQuery, List<SlicingJobDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllSlicingJobsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SlicingJobDto>> Handle(GetAllSlicingJobsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<SlicingJob>()
            .Include(sj => sj.SlicingJobStatus)
            .AsQueryable();

        if (request.DesignFileId.HasValue)
        {
            query = query.Where(sj => sj.DesignFileId == request.DesignFileId.Value);
        }

        if (request.SlicingJobStatusId.HasValue)
        {
            query = query.Where(sj => sj.SlicingJobStatusId == request.SlicingJobStatusId.Value);
        }

        var jobs = await query.ToListAsync(cancellationToken);

        return jobs.Select(j => new SlicingJobDto
        {
            Id = j.Id,
            DesignFileId = j.DesignFileId,
            SlicingProfileId = j.SlicingProfileId,
            SlicingJobStatusId = j.SlicingJobStatusId,
            SlicingJobStatusKey = j.SlicingJobStatus.Key,
            EstimatedTimeMin = j.EstimatedTimeMin,
            EstimatedMaterialGrams = j.EstimatedMaterialGrams,
            PriceEstimate = j.PriceEstimate,
            CompletedAt = j.CompletedAt,
            CreatedAt = j.CreatedAt,
            UpdatedAt = j.UpdatedAt
        }).ToList();
    }
}

