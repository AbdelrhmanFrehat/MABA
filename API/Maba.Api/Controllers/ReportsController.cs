using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Orders;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public ReportsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("Sales")]
    public async Task<ActionResult<IEnumerable<SalesReportRowDto>>> GetSalesReport(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var invoicesQuery = _context.Set<Invoice>()
            .Include(x => x.Order)
                .ThenInclude(x => x.User)
            .Include(x => x.Payments)
            .AsQueryable();

        if (fromDate.HasValue)
        {
            var from = fromDate.Value.Date;
            invoicesQuery = invoicesQuery.Where(x => x.IssueDate >= from);
        }

        if (toDate.HasValue)
        {
            var to = toDate.Value.Date.AddDays(1).AddTicks(-1);
            invoicesQuery = invoicesQuery.Where(x => x.IssueDate <= to);
        }

        var rows = await invoicesQuery
            .OrderByDescending(x => x.IssueDate)
            .Select(x => new SalesReportRowDto
            {
                InvoiceDate = x.IssueDate,
                InvoiceNumber = x.InvoiceNumber,
                CustomerNameEn = x.Order.User.FullName,
                CustomerNameAr = x.Order.User.FullName,
                TotalAmount = x.Total,
                DiscountAmount = x.Order.DiscountAmount,
                NetAmount = x.Total - x.Order.DiscountAmount,
                PaidAmount = x.Payments.Where(p => !p.IsRefunded).Sum(p => p.Amount),
                RemainingAmount = x.Total - x.Payments.Where(p => !p.IsRefunded).Sum(p => p.Amount)
            })
            .ToListAsync(cancellationToken);

        return Ok(rows);
    }

    public class SalesReportRowDto
    {
        public DateTime InvoiceDate { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string? CustomerNameAr { get; set; }
        public string? CustomerNameEn { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
    }
}
