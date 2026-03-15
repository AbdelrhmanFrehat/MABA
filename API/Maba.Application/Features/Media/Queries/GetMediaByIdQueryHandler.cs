using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Media.DTOs;
using Maba.Application.Features.Media.Queries;
using Maba.Domain.Media;

namespace Maba.Application.Features.Media.Handlers;

public class GetMediaByIdQueryHandler : IRequestHandler<GetMediaByIdQuery, MediaAssetDto>
{
    private readonly IApplicationDbContext _context;

    public GetMediaByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MediaAssetDto> Handle(GetMediaByIdQuery request, CancellationToken cancellationToken)
    {
        var mediaAsset = await _context.Set<MediaAsset>()
            .Include(m => m.MediaType)
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (mediaAsset == null)
        {
            throw new KeyNotFoundException("Media asset not found");
        }

        return new MediaAssetDto
        {
            Id = mediaAsset.Id,
            FileUrl = mediaAsset.FileUrl,
            MimeType = mediaAsset.MimeType,
            FileName = mediaAsset.FileName,
            FileExtension = mediaAsset.FileExtension,
            FileSizeBytes = mediaAsset.FileSizeBytes,
            Width = mediaAsset.Width,
            Height = mediaAsset.Height,
            TitleEn = mediaAsset.TitleEn,
            TitleAr = mediaAsset.TitleAr,
            AltEn = mediaAsset.AltEn,
            AltAr = mediaAsset.AltAr,
            UploadedByUserId = mediaAsset.UploadedByUserId,
            MediaTypeId = mediaAsset.MediaTypeId,
            MediaTypeKey = mediaAsset.MediaType.Key,
            CreatedAt = mediaAsset.CreatedAt,
            UpdatedAt = mediaAsset.UpdatedAt
        };
    }
}

