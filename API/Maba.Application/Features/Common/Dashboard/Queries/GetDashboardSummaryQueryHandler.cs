using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Common.Dashboard.DTOs;
using Maba.Application.Features.Common.Dashboard.Queries;
using Maba.Domain.Orders;
using Maba.Domain.Catalog;
using Maba.Domain.Printing;
using Maba.Domain.Users;

namespace Maba.Application.Features.Common.Dashboard.Handlers;

public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private readonly IApplicationDbContext _context;

    public GetDashboardSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var fromDate = request.FromDate ?? DateTime.UtcNow.AddMonths(-1);
        var toDate = request.ToDate ?? DateTime.UtcNow;

        // Total Orders
        var totalOrders = await _context.Set<Order>()
            .Where(o => o.CreatedAt >= fromDate && o.CreatedAt <= toDate)
            .CountAsync(cancellationToken);

        // Total Revenue
        var totalRevenue = await _context.Set<Order>()
            .Where(o => o.CreatedAt >= fromDate && o.CreatedAt <= toDate)
            .SumAsync(o => o.Total, cancellationToken);

        // Total Customers
        var totalCustomers = await _context.Set<User>()
            .CountAsync(cancellationToken);

        // Active 3D Jobs (Printing, Queued)
        var active3DJobs = await _context.Set<PrintJob>()
            .Include(pj => pj.PrintJobStatus)
            .Where(pj => pj.PrintJobStatus.Key == "Printing" || pj.PrintJobStatus.Key == "Queued")
            .CountAsync(cancellationToken);

        // Low Stock Items
        var lowStockItemsCount = await _context.Set<Inventory>()
            .Where(inv => inv.QuantityOnHand <= inv.ReorderLevel)
            .CountAsync(cancellationToken);

        // Pending Reviews (not approved yet)
        var pendingReviews = await _context.Set<Review>()
            .Where(r => !r.IsApproved)
            .CountAsync(cancellationToken);

        return new DashboardSummaryDto
        {
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue,
            TotalCustomers = totalCustomers,
            Active3DJobs = active3DJobs,
            LowStockItemsCount = lowStockItemsCount,
            PendingReviews = pendingReviews
        };
    }
}
