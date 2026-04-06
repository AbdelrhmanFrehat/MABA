using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Orders;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/sales-orders")]
[Authorize]
public class SalesOrdersController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public SalesOrdersController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SalesOrderDto>>> GetSalesOrders(CancellationToken cancellationToken)
    {
        var orders = await _context.Set<Order>()
            .Include(x => x.User)
            .Include(x => x.OrderStatus)
            .Include(x => x.OrderItems)
                .ThenInclude(x => x.Item)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new SalesOrderDto
            {
                Id = x.Id,
                OrderNumber = x.OrderNumber,
                CustomerId = x.UserId,
                CustomerName = !string.IsNullOrWhiteSpace(x.User.FullName) ? x.User.FullName : x.User.Email,
                StatusLookupId = x.OrderStatusId,
                StatusName = x.OrderStatus.NameEn,
                StatusColor = MapStatusColor(x.OrderStatus.Key),
                OrderDate = x.CreatedAt,
                ExpectedDeliveryDate = x.EstimatedDeliveryDate,
                Currency = x.Currency,
                SubTotal = x.SubTotal,
                DiscountAmount = x.DiscountAmount,
                TaxAmount = x.TaxAmount,
                Total = x.Total,
                ShippingAddress = x.ShippingAddress,
                Notes = x.Notes,
                CreatedByUserId = x.UserId,
                IsStorefrontOrder = x.IsStorefrontOrder,
                SourceLabel = x.IsStorefrontOrder ? "Website Order" : "Sales Order",
                Lines = x.OrderItems
                    .OrderBy(i => i.CreatedAt)
                    .Select((i, index) => new SalesOrderLineDto
                    {
                        SalesOrderId = x.Id,
                        LineNumber = index + 1,
                        ItemId = i.ItemId ?? Guid.Empty,
                        ItemName = i.Item != null ? i.Item.NameEn : null,
                        ItemSku = i.Item != null ? i.Item.Sku : null,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        DiscountAmount = i.DiscountAmount,
                        TaxAmount = i.TaxAmount,
                        LineTotal = i.LineTotal,
                        QuantityInvoiced = 0,
                        QuantityReturned = 0
                    })
                    .ToList(),
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(orders);
    }

    private static string MapStatusColor(string? statusKey)
    {
        var key = statusKey?.ToLowerInvariant() ?? string.Empty;

        if (key.Contains("cancel")) return "#ef4444";
        if (key.Contains("complete") || key.Contains("deliver")) return "#22c55e";
        if (key.Contains("process") || key.Contains("paid")) return "#10b981";
        if (key.Contains("pending") || key.Contains("new")) return "#f59e0b";
        return "#64748b";
    }

    public class SalesOrderDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public Guid StatusLookupId { get; set; }
        public string? StatusName { get; set; }
        public string? StatusColor { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string Currency { get; set; } = "ILS";
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal? DiscountPercent { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        public string? ShippingAddress { get; set; }
        public string? Notes { get; set; }
        public Guid CreatedByUserId { get; set; }
        public bool IsStorefrontOrder { get; set; }
        public string SourceLabel { get; set; } = string.Empty;
        public List<SalesOrderLineDto> Lines { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class SalesOrderLineDto
    {
        public Guid SalesOrderId { get; set; }
        public int LineNumber { get; set; }
        public Guid ItemId { get; set; }
        public string? ItemName { get; set; }
        public string? ItemSku { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }
        public decimal QuantityInvoiced { get; set; }
        public decimal QuantityReturned { get; set; }
    }
}
