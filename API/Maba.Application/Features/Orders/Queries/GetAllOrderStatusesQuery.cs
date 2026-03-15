using MediatR;
using Maba.Application.Features.Orders.DTOs;

namespace Maba.Application.Features.Orders.Queries;

public class GetAllOrderStatusesQuery : IRequest<List<OrderStatusDto>>
{
}
