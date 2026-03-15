using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Printing.Designs.Queries;
using Maba.Application.Features.Printing.DTOs;
using Maba.Domain.Printing;

namespace Maba.Application.Features.Printing.Designs.Handlers;

public class GetDesignDetailQueryHandler : IRequestHandler<GetDesignDetailQuery, DesignDetailDto>
{
    private readonly IApplicationDbContext _context;

    public GetDesignDetailQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DesignDetailDto> Handle(GetDesignDetailQuery request, CancellationToken cancellationToken)
    {
        var design = await _context.Set<Design>()
            .Include(d => d.User)
            .Include(d => d.DesignFiles)
            .ThenInclude(df => df.MediaAsset)
            .FirstOrDefaultAsync(d => d.Id == request.DesignId, cancellationToken);

        if (design == null)
        {
            throw new KeyNotFoundException("Design not found.");
        }

        var designFileIds = design.DesignFiles.Select(df => df.Id).ToList();
        var slicingJobsCount = await _context.Set<SlicingJob>()
            .CountAsync(sj => designFileIds.Contains(sj.DesignFileId), cancellationToken);

        var slicingJobIds = await _context.Set<SlicingJob>()
            .Where(sj => designFileIds.Contains(sj.DesignFileId))
            .Select(sj => sj.Id)
            .ToListAsync(cancellationToken);

        var printJobsCount = await _context.Set<PrintJob>()
            .CountAsync(pj => slicingJobIds.Contains(pj.SlicingJobId), cancellationToken);

        return new DesignDetailDto
        {
            Id = design.Id,
            UserId = design.UserId,
            UserFullName = design.User.FullName,
            Title = design.Title,
            Notes = design.Notes,
            IsPublic = design.IsPublic,
            Tags = design.Tags,
            LicenseType = design.LicenseType,
            DownloadCount = design.DownloadCount,
            LikeCount = design.LikeCount,
            Files = design.DesignFiles.Select(df => new DesignFileDto
            {
                Id = df.Id,
                DesignId = df.DesignId,
                MediaAssetId = df.MediaAssetId,
                FileUrl = df.MediaAsset.FileUrl,
                FileName = df.MediaAsset.FileName,
                Format = df.Format,
                FileSizeBytes = df.FileSizeBytes,
                IsPrimary = df.IsPrimary,
                UploadedAt = df.UploadedAt,
                CreatedAt = df.CreatedAt
            }).ToList(),
            SlicingJobsCount = slicingJobsCount,
            PrintJobsCount = printJobsCount,
            CreatedAt = design.CreatedAt,
            UpdatedAt = design.UpdatedAt
        };
    }
}

