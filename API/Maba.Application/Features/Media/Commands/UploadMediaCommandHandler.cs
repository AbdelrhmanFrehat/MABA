using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Media.Commands;
using Maba.Application.Features.Media.DTOs;
using Maba.Domain.Media;
using System.IO;

namespace Maba.Application.Features.Media.Handlers;

public class UploadMediaCommandHandler : IRequestHandler<UploadMediaCommand, MediaAssetDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public UploadMediaCommandHandler(
        IApplicationDbContext context,
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<MediaAssetDto> Handle(UploadMediaCommand request, CancellationToken cancellationToken)
    {
        // Validate media type (by Id, or for images fallback to type with Key "Image")
        var mediaType = await _context.Set<MediaType>()
            .FirstOrDefaultAsync(mt => mt.Id == request.MediaTypeId, cancellationToken);

        if (mediaType == null && request.File.ContentType.StartsWith("image/"))
        {
            mediaType = await _context.Set<MediaType>()
                .FirstOrDefaultAsync(mt => mt.Key == "Image", cancellationToken);
        }

        if (mediaType == null)
        {
            throw new KeyNotFoundException("Media type not found");
        }

        // Validate file
        if (request.File == null || request.File.Length == 0)
        {
            throw new InvalidOperationException("File is required");
        }

        // Generate unique file name
        var fileExtension = Path.GetExtension(request.File.FileName);
        var fileName = $"{Guid.NewGuid()}{fileExtension}";
        var webRootPath = _configuration["FileStorage:WebRootPath"] ?? "wwwroot";
        var uploadsPath = Path.Combine(webRootPath, "uploads", mediaType.Key);
        
        // Create directory if it doesn't exist
        Directory.CreateDirectory(uploadsPath);

        var filePath = Path.Combine(uploadsPath, fileName);
        var fileUrl = $"/uploads/{mediaType.Key}/{fileName}";

        // Save file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await request.File.CopyToAsync(stream, cancellationToken);
        }

        // Get image dimensions if it's an image
        int? width = null;
        int? height = null;
        if (request.File.ContentType.StartsWith("image/"))
        {
            try
            {
                using var image = System.Drawing.Image.FromFile(filePath);
                width = image.Width;
                height = image.Height;
            }
            catch
            {
                // Ignore if not an image or can't read dimensions
            }
        }

        // Create media asset
        var mediaAsset = new MediaAsset
        {
            Id = Guid.NewGuid(),
            FileUrl = fileUrl,
            MimeType = request.File.ContentType,
            FileName = request.File.FileName,
            FileExtension = fileExtension,
            FileSizeBytes = request.File.Length,
            Width = width,
            Height = height,
            TitleEn = request.TitleEn,
            TitleAr = request.TitleAr,
            AltEn = request.AltEn,
            AltAr = request.AltAr,
            UploadedByUserId = request.UploadedByUserId,
            MediaTypeId = mediaType.Id
        };

        _context.Set<MediaAsset>().Add(mediaAsset);
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
            MediaTypeKey = mediaType.Key,
            CreatedAt = mediaAsset.CreatedAt,
            UpdatedAt = mediaAsset.UpdatedAt
        };
    }
}

