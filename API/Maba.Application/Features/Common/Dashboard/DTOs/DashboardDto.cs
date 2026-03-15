namespace Maba.Application.Features.Common.Dashboard.DTOs;

public class DashboardSummaryDto
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalCustomers { get; set; }
    public int Active3DJobs { get; set; }
    public int LowStockItemsCount { get; set; }
    public int PendingReviews { get; set; }
}

public class SalesOverTimeDto
{
    public string Period { get; set; } = string.Empty;
    public decimal Sales { get; set; }
}

public class OrdersByStatusDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}
