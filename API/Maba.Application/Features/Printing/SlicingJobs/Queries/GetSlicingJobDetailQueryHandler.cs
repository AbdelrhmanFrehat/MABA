using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Printing.SlicingJobs.Queries;
using Maba.Application.Features.Printing.DTOs;
using Maba.Domain.Printing;

namespace Maba.Application.Features.Printing.SlicingJobs.Handlers;

public class GetSlicingJobDetailQueryHandler : IRequestHandler<GetSlicingJobDetailQuery, SlicingJobDetailDto>
{
    private readonly IApplicationDbContext _context;

    public GetSlicingJobDetailQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SlicingJobDetailDto> Handle(GetSlicingJobDetailQuery request, CancellationToken cancellationToken)
    {
        var slicingJob = await _context.Set<SlicingJob>()
            .Include(sj => sj.SlicingJobStatus)
            .Include(sj => sj.DesignFile)
            .ThenInclude(df => df.MediaAsset)
            .Include(sj => sj.SlicingProfile)
            .ThenInclude(sp => sp.Material)
            .Include(sj => sj.SlicingProfile)
            .ThenInclude(sp => sp.PrintingTechnology)
            .FirstOrDefaultAsync(sj => sj.Id == request.SlicingJobId, cancellationToken);

        if (slicingJob == null)
        {
            throw new KeyNotFoundException("Slicing job not found.");
        }

        var printJobsCount = await _context.Set<PrintJob>()
            .CountAsync(pj => pj.SlicingJobId == request.SlicingJobId, cancellationToken);

        return new SlicingJobDetailDto
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
            UpdatedAt = slicingJob.UpdatedAt,
            DesignFile = new DesignFileDto
            {
                Id = slicingJob.DesignFile.Id,
                DesignId = slicingJob.DesignFile.DesignId,
                MediaAssetId = slicingJob.DesignFile.MediaAssetId,
                FileUrl = slicingJob.DesignFile.MediaAsset.FileUrl,
                FileName = slicingJob.DesignFile.MediaAsset.FileName,
                Format = slicingJob.DesignFile.Format,
                FileSizeBytes = slicingJob.DesignFile.FileSizeBytes,
                IsPrimary = slicingJob.DesignFile.IsPrimary,
                UploadedAt = slicingJob.DesignFile.UploadedAt,
                CreatedAt = slicingJob.DesignFile.CreatedAt
            },
            SlicingProfile = new SlicingProfileDto
            {
                Id = slicingJob.SlicingProfile.Id,
                NameEn = slicingJob.SlicingProfile.NameEn,
                NameAr = slicingJob.SlicingProfile.NameAr,
                PrintingTechnologyId = slicingJob.SlicingProfile.PrintingTechnologyId,
                LayerHeightMm = slicingJob.SlicingProfile.LayerHeightMm,
                InfillPercent = slicingJob.SlicingProfile.InfillPercent,
                SupportsEnabled = slicingJob.SlicingProfile.SupportsEnabled,
                MaterialId = slicingJob.SlicingProfile.MaterialId,
                PrinterId = slicingJob.SlicingProfile.PrinterId,
                IsDefault = slicingJob.SlicingProfile.IsDefault,
                TemperatureSettings = slicingJob.SlicingProfile.TemperatureSettings,
                CreatedAt = slicingJob.SlicingProfile.CreatedAt,
                UpdatedAt = slicingJob.SlicingProfile.UpdatedAt
            },
            PrintJobsCount = printJobsCount
        };
    }
}

