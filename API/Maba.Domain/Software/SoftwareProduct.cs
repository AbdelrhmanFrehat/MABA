using Maba.Domain.Common;

namespace Maba.Domain.Software;

public class SoftwareProduct : BaseEntity
{
    public string Slug { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string SummaryEn { get; set; } = string.Empty;
    public string SummaryAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? Category { get; set; }
    public string? IconKey { get; set; }
    public string? LicenseEn { get; set; }
    public string? LicenseAr { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;

    public ICollection<SoftwareRelease> Releases { get; set; } = new List<SoftwareRelease>();
}
