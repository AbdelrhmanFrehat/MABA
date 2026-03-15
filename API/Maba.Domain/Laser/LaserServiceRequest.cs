using Maba.Domain.Common;

namespace Maba.Domain.Laser;

public class LaserServiceRequest : BaseEntity
{
    public string ReferenceNumber { get; set; } = string.Empty;
    
    public Guid MaterialId { get; set; }
    public LaserMaterial Material { get; set; } = null!;
    
    /// <summary>
    /// Operation mode: "cut" or "engrave"
    /// </summary>
    public string OperationMode { get; set; } = "engrave";
    
    /// <summary>
    /// Path to the uploaded image file
    /// </summary>
    public string ImagePath { get; set; } = string.Empty;
    
    /// <summary>
    /// Original filename of the uploaded image
    /// </summary>
    public string ImageFileName { get; set; } = string.Empty;
    
    /// <summary>
    /// Requested width in centimeters (max 40cm)
    /// </summary>
    public decimal? WidthCm { get; set; }
    
    /// <summary>
    /// Requested height in centimeters (max 40cm)
    /// </summary>
    public decimal? HeightCm { get; set; }
    
    /// <summary>
    /// Optional customer name
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
    /// Optional notes from customer
    /// </summary>
    public string? CustomerNotes { get; set; }
    
    /// <summary>
    /// Admin notes (internal)
    /// </summary>
    public string? AdminNotes { get; set; }
    
    /// <summary>
    /// Quoted price (set by admin)
    /// </summary>
    public decimal? QuotedPrice { get; set; }
    
    public LaserServiceRequestStatus Status { get; set; } = LaserServiceRequestStatus.Pending;
    
    /// <summary>
    /// When the request was reviewed
    /// </summary>
    public DateTime? ReviewedAt { get; set; }
    
    /// <summary>
    /// When the request was completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }
}
