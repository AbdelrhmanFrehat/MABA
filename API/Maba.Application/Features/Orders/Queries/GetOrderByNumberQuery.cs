using MediatR;
using Maba.Application.Features.Orders.DTOs;

namespace Maba.Application.Features.Orders.Queries;

public class GetOrderByNumberQuery : IRequest<OrderDto>
{
    public string OrderNumber { get; set; } = string.Empty;
}

