using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Printing.DTOs;
using Maba.Application.Features.Printing.Designs.Queries;
using Maba.Domain.Printing;
using System.IO;

namespace Maba.Application.Features.Printing.Designs.Handlers;

public class GetAllDesignsQueryHandler : IRequestHandler<GetAllDesignsQuery, List<DesignDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllDesignsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DesignDto>> Handle(GetAllDesignsQuery request, CancellationToken cancellationToken)
    {
        if (!request.UserId.HasValue)
            return new List<DesignDto>();

        var designs = await _context.Set<Design>()
            .Include(d => d.DesignFiles)
            .ThenInclude(df => df.MediaAsset)
            .Where(d => d.UserId == request.UserId.Value)
            .ToListAsync(cancellationToken);

        return designs.Select(d => new DesignDto
        {
            Id = d.Id,
            UserId = d.UserId,
            Title = d.Title,
            Notes = d.Notes,
            IsPublic = d.IsPublic,
            Tags = d.Tags,
            LicenseType = d.LicenseType,
            DownloadCount = d.DownloadCount,
            LikeCount = d.LikeCount,
            Files = d.DesignFiles.Select(df => new DesignFileDto
            {
                Id = df.Id,
                DesignId = df.DesignId,
                MediaAssetId = df.MediaAssetId,
                FileUrl = df.MediaAsset?.FileUrl ?? string.Empty,
                OriginalFileUrl = df.MediaAsset?.FileUrl ?? string.Empty,
                PreviewModelUrl = GetPreviewModelUrl(df.MediaAsset?.FileUrl, df.Format),
                PreviewFormat = GetPreviewFormat(df.Format),
                ThumbnailUrl = df.MediaAsset?.ThumbnailUrl,
                FileType = NormalizeFormat(df.Format, df.MediaAsset?.FileName),
                IsPreviewable = IsPreviewableFormat(df.Format),
                FileName = df.MediaAsset?.FileName ?? string.Empty,
                Format = df.Format,
                FileSizeBytes = df.FileSizeBytes,
                IsPrimary = df.IsPrimary,
                UploadedAt = df.UploadedAt,
                CreatedAt = df.CreatedAt
            }).ToList(),
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt
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

