namespace Maba.Application.Features.Media.DTOs;

/// <summary>
/// Represents a media file stream returned by the media service.
/// </summary>
public class MediaStreamResult : IAsyncDisposable
{
    public required Stream Stream { get; init; }
    public required string ContentType { get; init; }
    public string? FileName { get; init; }
    public bool IsPublic { get; init; }

    public ValueTask DisposeAsync()
    {
        Stream.Dispose();
        return ValueTask.CompletedTask;
    }
}

