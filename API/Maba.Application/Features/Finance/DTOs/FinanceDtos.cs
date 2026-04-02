namespace Maba.Application.Features.Finance.DTOs;

public class FinanceKpisDto
{
    public decimal TotalRevenue { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal Outstanding { get; set; }
    public decimal Refunds { get; set; }
}

public class FinanceChartDto
{
    public List<string> Labels { get; set; } = new();
    public List<decimal> Values { get; set; } = new();
}
