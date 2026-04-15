using Maba.Domain.Common;
using Maba.Domain.Crm;
using Maba.Domain.Users;

namespace Maba.Domain.DesignCad;

public class DesignCadServiceRequest : BaseEntity
{
    public string ReferenceNumber { get; set; } = string.Empty;

    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    /// <summary>FK to the first/primary quotation created from this request.</summary>
    public Guid? LinkedQuotationId { get; set; }

    public DesignCadRequestType RequestType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public DesignCadTargetProcess? TargetProcess { get; set; }
    public string? IntendedUse { get; set; }
    public string? MaterialNotes { get; set; }
    public string? DimensionsNotes { get; set; }
    public string? ToleranceNotes { get; set; }
    public string? WhatNeedsChange { get; set; }
    public string? CriticalSurfaces { get; set; }
    public string? FitmentRequirements { get; set; }
    public string? PurposeAndConstraints { get; set; }
    public string? Deadline { get; set; }

    public bool HasPhysicalPart { get; set; }
    public bool LegalConfirmation { get; set; }
    public bool CanDeliverPhysicalPart { get; set; }

    public string? CustomerNotes { get; set; }
    public string? AdminNotes { get; set; }
    public string? RejectionReason { get; set; }
    public decimal? QuotedPrice { get; set; }
    public decimal? FinalPrice { get; set; }

    public DesignCadRequestStatus Status { get; set; } = DesignCadRequestStatus.Pending;
    public DateTime? ReviewedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }

    public ICollection<DesignCadServiceRequestAttachment> Attachments { get; set; } = new List<DesignCadServiceRequestAttachment>();
}
