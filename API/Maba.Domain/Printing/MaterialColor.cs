using Maba.Domain.Common;

namespace Maba.Domain.Printing;

public class MaterialColor : BaseEntity
{
    public Guid MaterialId { get; set; }
    public Material Material { get; set; } = null!;
    
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string HexCode { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}
