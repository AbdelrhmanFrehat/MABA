using Maba.Domain.Common;

namespace Maba.Domain.Assets;

public class AssetNumberingSetting : BaseEntity
{
    public string Prefix { get; set; } = "A-";
    public int PadWidth { get; set; } = 4;
    public long NextNumber { get; set; } = 1;
}
