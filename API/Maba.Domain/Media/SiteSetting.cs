using Maba.Domain.Common;

namespace Maba.Domain.Media;

public class SiteSetting : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
    public Guid? UpdatedByUserId { get; set; }
}

