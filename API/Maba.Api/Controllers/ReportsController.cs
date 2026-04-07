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
        var ordersQuery = _context.Set<Order>()
            .Include(x => x.User)
            .Include(x => x.Payments)
            .Include(x => x.Invoices)
            .AsQueryable();

        if (fromDate.HasValue)
        {
            var from = fromDate.Value.Date;
            ordersQuery = ordersQuery.Where(x => x.CreatedAt >= from);
        }

        if (toDate.HasValue)
        {
            var to = toDate.Value.Date.AddDays(1).AddTicks(-1);
            ordersQuery = ordersQuery.Where(x => x.CreatedAt <= to);
        }

        var rows = await ordersQuery
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new SalesReportRowDto
            {
                InvoiceDate = x.CreatedAt,
                InvoiceNumber = x.Invoices.OrderByDescending(i => i.IssueDate).Select(i => i.InvoiceNumber).FirstOrDefault() ?? x.OrderNumber,
                CustomerNameEn = x.User.FullName ?? x.User.Email,
                CustomerNameAr = x.User.FullName ?? x.User.Email,
                TotalAmount = x.Total,
                DiscountAmount = x.DiscountAmount,
                NetAmount = x.Total - x.DiscountAmount,
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
