using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Media.Commands;
using Maba.Application.Features.Media.DTOs;
using Maba.Domain.Media;

namespace Maba.Application.Features.Media.Handlers;

public class UpdateMediaCommandHandler : IRequestHandler<UpdateMediaCommand, MediaAssetDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateMediaCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MediaAssetDto> Handle(UpdateMediaCommand request, CancellationToken cancellationToken)
    {
        var mediaAsset = await _context.Set<MediaAsset>()
            .Include(m => m.MediaType)
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (mediaAsset == null)
        {
            throw new KeyNotFoundException("Media asset not found");
        }

        if (request.TitleEn != null) mediaAsset.TitleEn = request.TitleEn;
        if (request.TitleAr != null) mediaAsset.TitleAr = request.TitleAr;
        if (request.AltEn != null) mediaAsset.AltEn = request.AltEn;
        if (request.AltAr != null) mediaAsset.AltAr = request.AltAr;
        
        mediaAsset.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

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

