using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Common.Models;
using Maba.Application.Features.Printing.Designs.Queries;
using Maba.Application.Features.Printing.DTOs;
using Maba.Domain.Printing;
using System.IO;

namespace Maba.Application.Features.Printing.Designs.Handlers;

public class SearchDesignsQueryHandler : IRequestHandler<SearchDesignsQuery, PagedResult<DesignDto>>
{
    private readonly IApplicationDbContext _context;

    public SearchDesignsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<DesignDto>> Handle(SearchDesignsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<Design>()
            .Include(d => d.User)
            .Include(d => d.DesignFiles)
            .ThenInclude(df => df.MediaAsset)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(d =>
                d.Title.ToLower().Contains(searchTerm) ||
                (d.Notes != null && d.Notes.ToLower().Contains(searchTerm)) ||
                (d.Tags != null && d.Tags.ToLower().Contains(searchTerm)));
        }

        if (request.UserId.HasValue)
        {
            query = query.Where(d => d.UserId == request.UserId.Value);
        }

        if (request.IsPublic.HasValue)
        {
            query = query.Where(d => d.IsPublic == request.IsPublic.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.LicenseType))
        {
            query = query.Where(d => d.LicenseType == request.LicenseType);
        }

        if (!string.IsNullOrWhiteSpace(request.Tags))
        {
            var tagList = request.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            query = query.Where(d => d.Tags != null && tagList.Any(tag => d.Tags.Contains(tag, StringComparison.OrdinalIgnoreCase)));
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "title" => request.SortDescending
                ? query.OrderByDescending(d => d.Title)
                : query.OrderBy(d => d.Title),
            "createdat" => request.SortDescending
                ? query.OrderByDescending(d => d.CreatedAt)
                : query.OrderBy(d => d.CreatedAt),
            "downloadcount" => request.SortDescending
                ? query.OrderByDescending(d => d.DownloadCount)
                : query.OrderBy(d => d.DownloadCount),
            "likecount" => request.SortDescending
                ? query.OrderByDescending(d => d.LikeCount)
                : query.OrderBy(d => d.LikeCount),
            _ => query.OrderByDescending(d => d.CreatedAt)
        };

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var designs = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var designsDto = designs.Select(d => new DesignDto
        {
            Id = d.Id,
            UserId = d.UserId,
            UserFullName = d.User.FullName,
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
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt
        }).ToList();

        return new PagedResult<DesignDto>
        {
            Items = designsDto,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
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

