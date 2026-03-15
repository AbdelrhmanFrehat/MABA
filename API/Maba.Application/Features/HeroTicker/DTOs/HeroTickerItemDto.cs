namespace Maba.Application.Features.HeroTicker.DTOs;

/// <summary>
/// Full DTO for admin list/detail.
/// </summary>
public class HeroTickerItemDto
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
