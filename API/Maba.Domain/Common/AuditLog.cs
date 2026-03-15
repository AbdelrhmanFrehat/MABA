using Maba.Domain.Users;

namespace Maba.Domain.Common;

public class AuditLog : BaseEntity
{
    public string EntityType { get; set; } = string.Empty; // e.g., "User", "Order", "Item"
    public Guid? EntityId { get; set; }
    public string Action { get; set; } = string.Empty; // e.g., "Create", "Update", "Delete", "View"
    public string? OldValues { get; set; } // JSON string
    public string? NewValues { get; set; } // JSON string
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? RequestPath { get; set; }
    public string? RequestMethod { get; set; }
    public int? StatusCode { get; set; }
    public string? ErrorMessage { get; set; }
    public long? DurationMs { get; set; }
    
    // Navigation properties
    public User? User { get; set; }
}

