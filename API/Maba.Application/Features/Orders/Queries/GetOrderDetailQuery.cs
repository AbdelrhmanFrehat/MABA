using MediatR;
using Maba.Application.Features.Orders.DTOs;

namespace Maba.Application.Features.Orders.Queries;

public class GetOrderDetailQuery : IRequest<OrderDto>
{
    public Guid OrderId { get; set; }
}

