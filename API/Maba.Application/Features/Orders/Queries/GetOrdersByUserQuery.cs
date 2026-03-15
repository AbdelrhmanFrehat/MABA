using MediatR;
using Maba.Application.Features.Orders.DTOs;

namespace Maba.Application.Features.Orders.Queries;

public class GetOrdersByUserQuery : IRequest<List<OrderDto>>
{
    public Guid UserId { get; set; }
    public Guid? OrderStatusId { get; set; }
}

