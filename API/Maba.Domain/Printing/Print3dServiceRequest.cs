using Maba.Domain.Common;
using Maba.Domain.Users;
using Maba.Domain.Media;

namespace Maba.Domain.Printing;

public class Print3dServiceRequest : BaseEntity
{
    public string ReferenceNumber { get; set; } = string.Empty;
    
    public Guid? UserId { get; set; }
    public User? User { get; set; }
    
    public Guid MaterialId { get; set; }
    public Material Material { get; set; } = null!;
    
    public Guid? MaterialColorId { get; set; }
    public MaterialColor? MaterialColor { get; set; }
    
    public Guid? ProfileId { get; set; }
    public PrintQualityProfile? Profile { get; set; }
    
    public Guid? DesignId { get; set; }
    public Design? Design { get; set; }
    
    /// <summary>
    /// Path to the uploaded STL file
    /// </summary>
    public string FilePath { get; set; } = string.Empty;
    
    /// <summary>
    /// Original filename of the uploaded STL file
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    
    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSizeBytes { get; set; }
    
    /// <summary>
    /// Optional customer name (for anonymous requests)
    /// </summary>
    public string? CustomerName { get; set; }
    
    /// <summary>
    /// Optional customer email
    /// </summary>
    public string? CustomerEmail { get; set; }
    
    /// <summary>
    /// Optional customer phone
    /// </summary>
    public string? CustomerPhone { get; set; }
    
    /// <summary>
    /// Customer notes/comments
    /// </summary>
    public string? CustomerNotes { get; set; }
    
    /// <summary>
    /// Admin notes (internal)
    /// </summary>
    public string? AdminNotes { get; set; }
    
    /// <summary>
    /// Estimated price (calculated or set by admin)
    /// </summary>
    public decimal? EstimatedPrice { get; set; }
    
    /// <summary>
    /// Final quoted price (confirmed by admin)
    /// </summary>
    public decimal? FinalPrice { get; set; }
    
    public string Currency { get; set; } = "ILS";
    
    public Print3dServiceRequestStatus Status { get; set; } = Print3dServiceRequestStatus.Pending;
    
    /// <summary>
    /// When the request was reviewed
    /// </summary>
    public DateTime? ReviewedAt { get; set; }
    
    /// <summary>
    /// When the request was approved
    /// </summary>
    public DateTime? ApprovedAt { get; set; }
    
    /// <summary>
    /// When the request was completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }
}
