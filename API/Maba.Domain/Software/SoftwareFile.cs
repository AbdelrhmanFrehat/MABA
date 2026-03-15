using Maba.Domain.Common;

namespace Maba.Domain.Software;

public class SoftwareFile : BaseEntity
{
    public Guid ReleaseId { get; set; }
    public SoftwareRelease Release { get; set; } = null!;

    public SoftwareFileOs Os { get; set; } = SoftwareFileOs.Windows;
    public SoftwareFileArch Arch { get; set; } = SoftwareFileArch.x64;
    public SoftwareFileType FileType { get; set; } = SoftwareFileType.Installer;

    public string FileName { get; set; } = string.Empty;
    public string StoredPath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string? Sha256 { get; set; }

    public int DownloadCount { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}
