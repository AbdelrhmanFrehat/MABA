using Maba.Application.Features.Media.DTOs;

namespace Maba.Application.Common.Interfaces;

/// <summary>
/// High-level media service used by the rest of the application.
/// Hides the underlying storage implementation (disk, S3, etc.).
/// </summary>
public interface IMediaService
{
    /// <summary>
    /// Returns a stream for the given media asset id, or null if not found.
    /// Authorization rules (public / private) should be enforced by the caller.
    /// </summary>
    Task<MediaStreamResult?> GetStreamAsync(Guid id, CancellationToken cancellationToken = default);
}

