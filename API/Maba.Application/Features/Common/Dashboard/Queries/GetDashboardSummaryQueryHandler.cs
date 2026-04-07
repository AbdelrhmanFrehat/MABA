using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Common.Dashboard.DTOs;
using Maba.Application.Features.Common.Dashboard.Queries;
using Maba.Domain.Orders;
using Maba.Domain.Catalog;
using Maba.Domain.Printing;
using Maba.Domain.Users;
using Maba.Domain.Design;
using Maba.Domain.DesignCad;
using Maba.Domain.Laser;
using Maba.Domain.Cnc;
using Maba.Domain.Projects;

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

        const string cancelledKey = "Cancelled";

        // Total Orders — all orders created in range (includes cancelled)
        var totalOrders = await _context.Set<Order>()
            .Where(o => o.CreatedAt >= fromDate && o.CreatedAt <= toDate)
            .CountAsync(cancellationToken);

        // Total Revenue — paid-in-full, non-cancelled (matches order detail payment logic)
        var totalRevenue = await _context.Set<Order>()
            .Where(o => o.CreatedAt >= fromDate && o.CreatedAt <= toDate)
            .Where(o => o.OrderStatus.Key != cancelledKey)
            .Where(o => o.Payments.Sum(p => p.Amount) >= o.Total)
            .SumAsync(o => o.Total, cancellationToken);

        var totalRequests =
            await _context.Set<ProjectRequest>().Where(x => x.CreatedAt >= fromDate && x.CreatedAt <= toDate).CountAsync(cancellationToken) +
            await _context.Set<Print3dServiceRequest>().Where(x => x.CreatedAt >= fromDate && x.CreatedAt <= toDate).CountAsync(cancellationToken) +
            await _context.Set<DesignServiceRequest>().Where(x => x.CreatedAt >= fromDate && x.CreatedAt <= toDate).CountAsync(cancellationToken) +
            await _context.Set<DesignCadServiceRequest>().Where(x => x.CreatedAt >= fromDate && x.CreatedAt <= toDate).CountAsync(cancellationToken) +
            await _context.Set<LaserServiceRequest>().Where(x => x.CreatedAt >= fromDate && x.CreatedAt <= toDate).CountAsync(cancellationToken) +
            await _context.Set<CncServiceRequest>().Where(x => x.CreatedAt >= fromDate && x.CreatedAt <= toDate).CountAsync(cancellationToken);

        // Total Customers — users registered in the selected range (aligns with date filter)
        var totalCustomers = await _context.Set<User>()
            .Where(u => u.CreatedAt >= fromDate && u.CreatedAt <= toDate)
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
            TotalRequests = totalRequests,
            TotalRevenue = totalRevenue,
            TotalCustomers = totalCustomers,
            Active3DJobs = active3DJobs,
            LowStockItemsCount = lowStockItemsCount,
            PendingReviews = pendingReviews
        };
    }
}
