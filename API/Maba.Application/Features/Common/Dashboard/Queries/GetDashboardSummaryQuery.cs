using MediatR;
using Maba.Application.Features.Common.Dashboard.DTOs;

namespace Maba.Application.Features.Common.Dashboard.Queries;

public class GetDashboardSummaryQuery : IRequest<DashboardSummaryDto>
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
