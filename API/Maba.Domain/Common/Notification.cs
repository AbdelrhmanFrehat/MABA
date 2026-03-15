using Maba.Domain.Users;

namespace Maba.Domain.Common;

public class Notification : BaseEntity
{
    public Guid? UserId { get; set; } // null = broadcast notification
    public string Type { get; set; } = string.Empty; // e.g., "Info", "Warning", "Error", "Success"
    public string TitleEn { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string MessageEn { get; set; } = string.Empty;
    public string MessageAr { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public string? Icon { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; } // e.g., "Order", "Item", "Payment"
    
    // Navigation properties
    public User? User { get; set; }
}

