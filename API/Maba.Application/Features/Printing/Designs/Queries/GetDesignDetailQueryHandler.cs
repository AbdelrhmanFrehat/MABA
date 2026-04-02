using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Printing.Designs;
using Maba.Application.Features.Printing.Designs.Queries;
using Maba.Application.Features.Printing.DTOs;
using Maba.Domain.Printing;
using System.IO;

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

        if (!DesignVisibility.CanView(design, request.RequestingUserId, request.IsPrivileged))
            throw new KeyNotFoundException("Design not found.");

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
                OriginalFileUrl = df.MediaAsset.FileUrl,
                PreviewModelUrl = GetPreviewModelUrl(df.MediaAsset.FileUrl, df.Format),
                PreviewFormat = GetPreviewFormat(df.Format),
                ThumbnailUrl = df.MediaAsset.ThumbnailUrl,
                FileType = NormalizeFormat(df.Format, df.MediaAsset.FileName),
                IsPreviewable = IsPreviewableFormat(df.Format),
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

    private static string NormalizeFormat(string? format, string? fileName)
    {
        var normalized = (format ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalized) && !string.IsNullOrWhiteSpace(fileName))
        {
            normalized = Path.GetExtension(fileName).TrimStart('.');
        }
        return normalized.ToUpperInvariant();
    }

    private static bool IsPreviewableFormat(string? format)
    {
        var value = NormalizeFormat(format, null);
        return value is "GLB" or "GLTF" or "STL" or "OBJ";
    }

    private static string? GetPreviewModelUrl(string? fileUrl, string? format)
    {
        return IsPreviewableFormat(format) ? fileUrl : null;
    }

    private static string? GetPreviewFormat(string? format)
    {
        var value = NormalizeFormat(format, null);
        return IsPreviewableFormat(value) ? value : null;
    }
}

