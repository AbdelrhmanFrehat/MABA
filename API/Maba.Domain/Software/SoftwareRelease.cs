using Maba.Domain.Common;

namespace Maba.Domain.Software;

public class SoftwareRelease : BaseEntity
{
    public Guid ProductId { get; set; }
    public SoftwareProduct Product { get; set; } = null!;

    public string Version { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; } = DateTime.UtcNow;
    public SoftwareReleaseStatus Status { get; set; } = SoftwareReleaseStatus.Stable;

    public string? ChangelogEn { get; set; }
    public string? ChangelogAr { get; set; }
    public string? RequirementsEn { get; set; }
    public string? RequirementsAr { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<SoftwareFile> Files { get; set; } = new List<SoftwareFile>();
}
