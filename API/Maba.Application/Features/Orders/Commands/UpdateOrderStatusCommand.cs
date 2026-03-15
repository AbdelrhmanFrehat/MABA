using MediatR;
using Maba.Application.Features.Orders.DTOs;

namespace Maba.Application.Features.Orders.Commands;

public class UpdateOrderStatusCommand : IRequest<OrderDto>
{
    public Guid OrderId { get; set; }
    public Guid OrderStatusId { get; set; }
    public string? TrackingNumber { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
}

