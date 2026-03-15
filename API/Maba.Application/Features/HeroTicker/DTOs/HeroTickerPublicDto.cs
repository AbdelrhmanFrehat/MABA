namespace Maba.Application.Features.HeroTicker.DTOs;

/// <summary>
/// Minimal DTO for public hero ticker (active, ordered).
/// </summary>
public class HeroTickerPublicDto
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
