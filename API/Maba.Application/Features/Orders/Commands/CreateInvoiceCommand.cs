using MediatR;
using Maba.Application.Features.Orders.DTOs;

namespace Maba.Application.Features.Orders.Commands;

public class CreateInvoiceCommand : IRequest<InvoiceDto>
{
    public Guid OrderId { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? DueDate { get; set; }
}

