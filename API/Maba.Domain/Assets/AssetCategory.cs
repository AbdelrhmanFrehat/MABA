using Maba.Domain.Common;

namespace Maba.Domain.Assets;

public class AssetCategory : BaseEntity
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? NumberingPrefix { get; set; }
    public bool IsActive { get; set; } = true;
}
