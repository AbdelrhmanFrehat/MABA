using MediatR;
using Maba.Application.Features.Common.Dashboard.DTOs;

namespace Maba.Application.Features.Common.Dashboard.Queries;

public class GetSalesOverTimeQuery : IRequest<List<SalesOverTimeDto>>
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Periods { get; set; } = 6; // Number of periods to return
}
