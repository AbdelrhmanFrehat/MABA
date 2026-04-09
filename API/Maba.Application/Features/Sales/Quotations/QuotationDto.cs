namespace Maba.Application.Features.Sales.Quotations;

public class QuotationDto
{
    public Guid Id { get; set; }
    public string QuotationNumber { get; set; } = string.Empty;

    // Customer
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerCode { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }

    // Source request (optional)
    public Guid? SourceRequestId { get; set; }
    public string? SourceRequestType { get; set; }
    public string? SourceRequestReference { get; set; }

    // Status
    public string Status { get; set; } = "Draft";
    public string? StatusColor { get; set; }

    // Dates
    public DateTime QuotationDate { get; set; }
    public DateTime? ValidUntil { get; set; }

    // Financials
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public string Currency { get; set; } = "ILS";

    // Content
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
    public string? TermsAndConditions { get; set; }

    // Conversion
    public Guid? ConvertedToOrderId { get; set; }
    public string? ConvertedToOrderNumber { get; set; }
    public DateTime? ConvertedAt { get; set; }

    // Audit
    public Guid? CreatedByUserId { get; set; }

    public List<QuotationItemDto> Items { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class QuotationItemDto
{
    public Guid Id { get; set; }
    public Guid QuotationId { get; set; }
    public int LineNumber { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "pcs";
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxPercent { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
    public string? Notes { get; set; }
}

// ── Request/Response DTOs ────────────────────────────────────────────────────

public class CreateQuotationRequest
{
    public Guid CustomerId { get; set; }
    public DateTime? QuotationDate { get; set; }
    public DateTime? ValidUntil { get; set; }
    public string Currency { get; set; } = "ILS";
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
    public string? TermsAndConditions { get; set; }
    public List<QuotationItemRequest> Items { get; set; } = new();
}

public class CreateQuotationFromRequestRequest
{
    public string RequestType { get; set; } = string.Empty;
    public Guid RequestId { get; set; }
    public DateTime? ValidUntil { get; set; }
    public string Currency { get; set; } = "ILS";
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
    public string? TermsAndConditions { get; set; }
    public List<QuotationItemRequest> Items { get; set; } = new();
}

public class QuotationItemRequest
{
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1;
    public string Unit { get; set; } = "pcs";
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal TaxPercent { get; set; }
    public string? Notes { get; set; }
}

public class UpdateQuotationRequest
{
    public DateTime? ValidUntil { get; set; }
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
    public string? TermsAndConditions { get; set; }
    public string? Status { get; set; }
    public List<QuotationItemRequest>? Items { get; set; }
}

// ── Commercial Links ─────────────────────────────────────────────────────────

public class RequestCommercialLinksDto
{
    public Guid RequestId { get; set; }
    public string RequestType { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public bool CanCreateQuotation { get; set; }
    public string? BlockedReason { get; set; }
    public List<CommercialDocLinkDto> Quotations { get; set; } = new();
    public List<CommercialDocLinkDto> Orders { get; set; } = new();
    public List<CommercialDocLinkDto> Invoices { get; set; } = new();
}

public class CommercialDocLinkDto
{
    public Guid Id { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? StatusColor { get; set; }
    public decimal Total { get; set; }
    public string Currency { get; set; } = "ILS";
    public DateTime CreatedAt { get; set; }
}

// ── Commercial Draft ─────────────────────────────────────────────────────────

public class RequestCommercialDraftDto
{
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string? InternalNotes { get; set; }
    public bool CanCreateQuotation { get; set; }
    public string? BlockedReason { get; set; }
    public bool HasExistingQuotation { get; set; }
    public Guid? ExistingQuotationId { get; set; }
    public string? ExistingQuotationNumber { get; set; }
    public string? ExistingQuotationStatus { get; set; }
    public List<QuotationItemDraftDto> DefaultItems { get; set; } = new();
}

public class QuotationItemDraftDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1;
    public string Unit { get; set; } = "pcs";
    public decimal UnitPrice { get; set; } = 0;
}
