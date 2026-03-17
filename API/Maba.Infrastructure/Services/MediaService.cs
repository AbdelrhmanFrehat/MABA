using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Media.DTOs;
using Maba.Domain.Media;
using Maba.Infrastructure.Data;

namespace Maba.Infrastructure.Services;

/// <summary>
/// Default implementation of IMediaService that uses the database for metadata
/// and IFileStorageService for the physical files (currently local disk).
/// </summary>
public class MediaService : IMediaService
{
    private readonly ApplicationDbContext _db;
    private readonly IFileStorageService _fileStorage;

    public MediaService(ApplicationDbContext db, IFileStorageService fileStorage)
    {
        _db = db;
        _fileStorage = fileStorage;
    }

    public async Task<MediaStreamResult?> GetStreamAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var media = await _db.Set<MediaAsset>()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (media is null)
        {
            return null;
        }

        // Prefer StorageKey if set (future-proof for non-local providers)
        string? storagePath = null;
        if (!string.IsNullOrWhiteSpace(media.StorageKey))
        {
            storagePath = media.StorageKey;
        }
        else if (!string.IsNullOrWhiteSpace(media.FileUrl))
        {
            // Existing data stores a web URL like: /uploads/{typeKey}/{fileName}
            // We need to convert it back to a relative storage path for IFileStorageService.
            var fileUrl = media.FileUrl.TrimStart('/');
            const string uploadsPrefix = "uploads/";
            if (fileUrl.StartsWith(uploadsPrefix, StringComparison.OrdinalIgnoreCase))
            {
                storagePath = fileUrl[uploadsPrefix.Length..];
            }
            else
            {
                // Fallback: use the whole trimmed url as relative path
                storagePath = fileUrl;
            }
        }

        if (string.IsNullOrWhiteSpace(storagePath))
        {
            return null;
        }

        var stream = await _fileStorage.GetFileAsync(storagePath);
        if (stream is null)
        {
            return null;
        }

        var contentType = string.IsNullOrWhiteSpace(media.MimeType)
            ? "application/octet-stream"
            : media.MimeType;

        return new MediaStreamResult
        {
            Stream = stream,
            ContentType = contentType,
            FileName = media.FileName,
            IsPublic = media.IsPublic
        };
    }
}

