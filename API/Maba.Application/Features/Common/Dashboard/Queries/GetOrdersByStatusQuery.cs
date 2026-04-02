using MediatR;
using Maba.Application.Features.Common.Dashboard.DTOs;

namespace Maba.Application.Features.Common.Dashboard.Queries;

public class GetOrdersByStatusQuery : IRequest<List<OrdersByStatusDto>>
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
