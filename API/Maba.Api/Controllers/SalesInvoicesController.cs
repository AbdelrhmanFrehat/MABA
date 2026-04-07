using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Orders;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/sales-invoices")]
[Authorize]
public class SalesInvoicesController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public SalesInvoicesController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SalesInvoiceDto>>> GetSalesInvoices(
        [FromQuery] Guid? customerId,
        CancellationToken cancellationToken)
    {
        var query = _context.Set<Invoice>()
            .Include(x => x.Order)
                .ThenInclude(x => x.User)
            .Include(x => x.InvoiceStatus)
            .Include(x => x.Payments)
            .AsQueryable();

        if (customerId.HasValue)
            query = query.Where(x => x.Order.UserId == customerId.Value);

        var invoices = await query
            .OrderByDescending(x => x.IssueDate)
            .Select(x => new SalesInvoiceDto
            {
                Id = x.Id,
                InvoiceNumber = x.InvoiceNumber,
                CustomerId = x.Order.UserId,
                CustomerName = x.Order.User.FullName ?? x.Order.User.Email,
                StatusLookupId = x.InvoiceStatusId,
                StatusName = x.InvoiceStatus.Key,
                SalesOrderId = x.OrderId,
                SalesOrderNumber = x.Order.OrderNumber,
                InvoiceDate = x.IssueDate,
                DueDate = x.DueDate,
                Currency = x.Currency,
                SubTotal = x.Order.SubTotal,
                DiscountAmount = x.Order.DiscountAmount,
                TaxAmount = x.Order.TaxAmount,
                Total = x.Total,
                AmountPaid = x.Payments.Where(p => !p.IsRefunded).Sum(p => p.Amount),
                AmountDue = x.Total - x.Payments.Where(p => !p.IsRefunded).Sum(p => p.Amount),
                IsPosted = true,
                PdfUrl = x.PdfUrl,
                CreatedByUserId = x.Order.UserId,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(invoices);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SalesInvoiceDto>> GetSalesInvoice(Guid id, CancellationToken cancellationToken)
    {
        var x = await _context.Set<Invoice>()
            .Include(i => i.Order).ThenInclude(o => o.User)
            .Include(i => i.InvoiceStatus)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        if (x == null) return NotFound();

        return Ok(new SalesInvoiceDto
        {
            Id = x.Id,
            InvoiceNumber = x.InvoiceNumber,
            CustomerId = x.Order.UserId,
            CustomerName = x.Order.User.FullName ?? x.Order.User.Email,
            StatusLookupId = x.InvoiceStatusId,
            StatusName = x.InvoiceStatus.Key,
            SalesOrderId = x.OrderId,
            SalesOrderNumber = x.Order.OrderNumber,
            InvoiceDate = x.IssueDate,
            DueDate = x.DueDate,
            Currency = x.Currency,
            SubTotal = x.Order.SubTotal,
            DiscountAmount = x.Order.DiscountAmount,
            TaxAmount = x.Order.TaxAmount,
            Total = x.Total,
            AmountPaid = x.Payments.Where(p => !p.IsRefunded).Sum(p => p.Amount),
            AmountDue = x.Total - x.Payments.Where(p => !p.IsRefunded).Sum(p => p.Amount),
            IsPosted = true,
            PdfUrl = x.PdfUrl,
            CreatedByUserId = x.Order.UserId,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        });
    }

    public class SalesInvoiceDto
    {
        public Guid Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public Guid StatusLookupId { get; set; }
        public string? StatusName { get; set; }
        public string? StatusColor { get; set; }
        public Guid SalesOrderId { get; set; }
        public string? SalesOrderNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string Currency { get; set; } = "ILS";
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal AmountDue { get; set; }
        public bool IsPosted { get; set; }
        public string? PdfUrl { get; set; }
        public string? Notes { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
