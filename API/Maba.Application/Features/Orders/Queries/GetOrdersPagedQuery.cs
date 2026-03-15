using MediatR;
using Maba.Application.Common.Models;
using Maba.Application.Features.Orders.DTOs;

namespace Maba.Application.Features.Orders.Queries;

public class GetOrdersPagedQuery : IRequest<PagedResult<OrderDto>>
{
    public Guid? UserId { get; set; }
    public Guid? OrderStatusId { get; set; }
    public string? PaymentStatus { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;
}
