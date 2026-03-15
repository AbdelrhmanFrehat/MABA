using MediatR;
using Maba.Application.Features.Orders.DTOs;

namespace Maba.Application.Features.Orders.Queries;

public class GetAllPaymentsQuery : IRequest<List<PaymentDto>>
{
    public Guid? OrderId { get; set; }
    public Guid? InvoiceId { get; set; }
}

