using Maba.Domain.Common;
using Maba.Domain.Crm;
using Maba.Domain.Users;

namespace Maba.Domain.Cnc;

public enum CncServiceRequestStatus
{
    Pending,
    InReview,
    Quoted,
    Accepted,
    InProgress,
    Completed,
    Cancelled,
    Rejected
}

public class CncServiceRequest : BaseEntity
{
    public string ReferenceNumber { get; set; } = string.Empty;
    
    public string ServiceMode { get; set; } = string.Empty;
    
    public Guid? MaterialId { get; set; }
    public CncMaterial? Material { get; set; }
    
    public string? PcbMaterial { get; set; }
    public decimal? PcbThickness { get; set; }
    public string? PcbSide { get; set; }
    public string? PcbOperation { get; set; }
    
    public string? OperationType { get; set; }
    
    public decimal? WidthMm { get; set; }
    public decimal? HeightMm { get; set; }
    public decimal? ThicknessMm { get; set; }
    public int Quantity { get; set; } = 1;
    
    public string? DepthMode { get; set; }
    public decimal? DepthMm { get; set; }
    
    public string DesignSourceType { get; set; } = "production";
    public string? FilePath { get; set; }
    public string? FileName { get; set; }
    public string? DesignNotes { get; set; }
    
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string? ProjectDescription { get; set; }

    public Guid? UserId { get; set; }
    public User? User { get; set; }
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    /// <summary>FK to the first/primary quotation created from this request.</summary>
    public Guid? LinkedQuotationId { get; set; }
    
    public string? AdminNotes { get; set; }
    public string? RejectionReason { get; set; }
    public decimal? EstimatedPrice { get; set; }
    public decimal? FinalPrice { get; set; }
    
    public CncServiceRequestStatus Status { get; set; } = CncServiceRequestStatus.Pending;
    
    public DateTime? ReviewedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
