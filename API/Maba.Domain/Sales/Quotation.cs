using Maba.Domain.Common;
using Maba.Domain.Crm;
using Maba.Domain.Orders;

namespace Maba.Domain.Sales;

/// <summary>
/// Commercial quotation document – bridges engineering service requests to the ERP sales pipeline.
/// A quotation is always linked to an ERP Customer (not a website User directly).
/// It can optionally originate from a service request (project, CNC, laser, 3D print, design, CAD).
/// </summary>
public class Quotation : BaseEntity
{
    /// <summary>System-generated number, e.g. QTN-2026-0001.</summary>
    public string QuotationNumber { get; set; } = string.Empty;

    // ── CRM Customer ─────────────────────────────────────────────────────────
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    // ── Source request linkage (optional) ────────────────────────────────────
    /// <summary>FK of the originating service request (any type).</summary>
    public Guid? SourceRequestId { get; set; }

    /// <summary>'project' | 'cnc' | 'laser' | 'print3d' | 'design' | 'designCad'</summary>
    public string? SourceRequestType { get; set; }

    /// <summary>Cached reference number for display without joining all 6 request tables.</summary>
    public string? SourceRequestReference { get; set; }

    // ── Status ────────────────────────────────────────────────────────────────
    public QuotationStatus Status { get; set; } = QuotationStatus.Draft;

    // ── Dates ─────────────────────────────────────────────────────────────────
    public DateTime QuotationDate { get; set; }
    public DateTime? ValidUntil { get; set; }

    // ── Financials ────────────────────────────────────────────────────────────
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public string Currency { get; set; } = "ILS";

    // ── Content ───────────────────────────────────────────────────────────────
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
    public string? TermsAndConditions { get; set; }

    // ── Conversion to Sales Order ─────────────────────────────────────────────
    public Guid? ConvertedToOrderId { get; set; }
    public Order? ConvertedToOrder { get; set; }
    public DateTime? ConvertedAt { get; set; }

    // ── Audit ─────────────────────────────────────────────────────────────────
    public Guid? CreatedByUserId { get; set; }

    // ── Navigation ────────────────────────────────────────────────────────────
    public ICollection<QuotationItem> Items { get; set; } = new List<QuotationItem>();
}
