using Maba.Domain.Common;

namespace Maba.Domain.Announcements;

public class AppAnnouncementItem : BaseEntity
{
    public string Message { get; set; } = string.Empty;
    public string? Type { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;
    public string TargetPlatform { get; set; } = "All";
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
}
