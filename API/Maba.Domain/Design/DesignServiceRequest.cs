using Maba.Domain.Common;
using Maba.Domain.Users;

namespace Maba.Domain.Design;

public class DesignServiceRequest : BaseEntity
{
    public string ReferenceNumber { get; set; } = string.Empty;

    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public DesignServiceRequestType RequestType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IntendedUse { get; set; }
    public string? MaterialPreference { get; set; }
    public string? DimensionsNotes { get; set; }
    public ToleranceLevel? ToleranceLevel { get; set; }
    public string? BudgetRange { get; set; }
    public string? Timeline { get; set; }
    public bool IpOwnershipConfirmed { get; set; }

    public DesignServiceRequestStatus Status { get; set; } = DesignServiceRequestStatus.New;

    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }

    public string? AdminNotes { get; set; }
    public decimal? QuotedPrice { get; set; }
    public decimal? FinalPrice { get; set; }
    public string? DeliveryNotes { get; set; }

    public DateTime? ReviewedAt { get; set; }
    public DateTime? QuotedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }

    public ICollection<DesignServiceRequestAttachment> Attachments { get; set; } = new List<DesignServiceRequestAttachment>();
}
