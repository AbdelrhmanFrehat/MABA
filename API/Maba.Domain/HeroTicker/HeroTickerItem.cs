using Maba.Domain.Common;

namespace Maba.Domain.HeroTicker;

public class HeroTickerItem : BaseEntity
{
    public string? Title { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
