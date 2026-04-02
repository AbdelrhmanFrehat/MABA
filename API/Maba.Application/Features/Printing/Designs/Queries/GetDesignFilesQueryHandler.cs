using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Printing.Designs;
using Maba.Application.Features.Printing.Designs.Queries;
using Maba.Application.Features.Printing.DTOs;
using Maba.Domain.Printing;
using System.IO;

namespace Maba.Application.Features.Printing.Designs.Handlers;

public class GetDesignFilesQueryHandler : IRequestHandler<GetDesignFilesQuery, List<DesignFileDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDesignFilesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DesignFileDto>> Handle(GetDesignFilesQuery request, CancellationToken cancellationToken)
    {
        var design = await _context.Set<Design>()
            .FirstOrDefaultAsync(d => d.Id == request.DesignId, cancellationToken);

        if (design == null)
        {
            throw new KeyNotFoundException("Design not found.");
        }

        if (!DesignVisibility.CanView(design, request.RequestingUserId, request.IsPrivileged))
            throw new KeyNotFoundException("Design not found.");

        var files = await _context.Set<DesignFile>()
            .Include(df => df.MediaAsset)
            .Where(df => df.DesignId == request.DesignId)
            .OrderBy(df => df.IsPrimary ? 0 : 1)
            .ThenBy(df => df.UploadedAt)
            .ToListAsync(cancellationToken);

        return files.Select(df => new DesignFileDto
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
        }).ToList();
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

