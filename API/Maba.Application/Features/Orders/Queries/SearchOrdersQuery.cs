using MediatR;
using Maba.Application.Features.Orders.DTOs;

namespace Maba.Application.Features.Orders.Queries;

public class SearchOrdersQuery : IRequest<List<OrderDto>>
{
    public string? OrderNumber { get; set; }
    public Guid? UserId { get; set; }
    public Guid? OrderStatusId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

