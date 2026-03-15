using MediatR;
using Maba.Application.Features.Orders.DTOs;

namespace Maba.Application.Features.Orders.Commands;

public class CancelOrderCommand : IRequest<OrderDto>
{
    public Guid OrderId { get; set; }
    public string? Reason { get; set; }
}

