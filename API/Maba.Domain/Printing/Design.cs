using Maba.Domain.Common;
using Maba.Domain.Users;

namespace Maba.Domain.Printing;

public class Design : BaseEntity
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool IsPublic { get; set; } = false;
    public string? Tags { get; set; } // Comma-separated tags
    public string? LicenseType { get; set; } // e.g., "CC-BY", "Commercial", "Private"
    public int DownloadCount { get; set; } = 0;
    public int LikeCount { get; set; } = 0;
    
    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<DesignFile> DesignFiles { get; set; } = new List<DesignFile>();
}

