using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Media.DTOs;
using Maba.Application.Features.Media.Queries;
using Maba.Domain.Media;

namespace Maba.Application.Features.Media.Handlers;

public class GetAllMediaQueryHandler : IRequestHandler<GetAllMediaQuery, List<MediaAssetDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllMediaQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MediaAssetDto>> Handle(GetAllMediaQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<MediaAsset>()
            .Include(m => m.MediaType)
            .AsQueryable();

        if (request.MediaTypeId.HasValue)
        {
            query = query.Where(m => m.MediaTypeId == request.MediaTypeId.Value);
        }

        if (request.UploadedByUserId.HasValue)
        {
            query = query.Where(m => m.UploadedByUserId == request.UploadedByUserId.Value);
        }

        var mediaAssets = await query.ToListAsync(cancellationToken);

        return mediaAssets.Select(m => new MediaAssetDto
        {
            Id = m.Id,
            FileUrl = m.FileUrl,
            MimeType = m.MimeType,
            FileName = m.FileName,
            FileExtension = m.FileExtension,
            FileSizeBytes = m.FileSizeBytes,
            Width = m.Width,
            Height = m.Height,
            TitleEn = m.TitleEn,
            TitleAr = m.TitleAr,
            AltEn = m.AltEn,
            AltAr = m.AltAr,
            UploadedByUserId = m.UploadedByUserId,
            MediaTypeId = m.MediaTypeId,
            MediaTypeKey = m.MediaType.Key,
            CreatedAt = m.CreatedAt,
            UpdatedAt = m.UpdatedAt
        }).ToList();
    }
}

