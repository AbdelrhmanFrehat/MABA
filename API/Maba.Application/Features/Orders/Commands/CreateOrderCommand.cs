using MediatR;
using Maba.Application.Features.Orders.DTOs;

namespace Maba.Application.Features.Orders.Commands;

public class CreateOrderCommand : IRequest<OrderDto>
{
    public Guid UserId { get; set; }
    public List<OrderItemInput> OrderItems { get; set; } = new();
    public string? ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }
    public Guid? ShippingMethodId { get; set; }
    public string? Notes { get; set; }
}

public class OrderItemInput
{
    public Guid ItemId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? MetaJson { get; set; }
}

