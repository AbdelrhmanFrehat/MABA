using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Printing.DTOs;
using Maba.Application.Features.Printing.Designs.Queries;
using Maba.Domain.Printing;

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
        var query = _context.Set<Design>()
            .Include(d => d.DesignFiles)
            .ThenInclude(df => df.MediaAsset)
            .AsQueryable();

        if (request.UserId.HasValue)
        {
            query = query.Where(d => d.UserId == request.UserId.Value);
        }

        var designs = await query.ToListAsync(cancellationToken);

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
}

