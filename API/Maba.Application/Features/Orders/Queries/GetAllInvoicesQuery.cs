using MediatR;
using Maba.Application.Features.Orders.DTOs;

namespace Maba.Application.Features.Orders.Queries;

public class GetAllInvoicesQuery : IRequest<List<InvoiceDto>>
{
    public Guid? OrderId { get; set; }
    public Guid? InvoiceStatusId { get; set; }
}

