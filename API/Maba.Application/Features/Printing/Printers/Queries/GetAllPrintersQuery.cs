using MediatR;
using Maba.Application.Features.Printing.DTOs;

namespace Maba.Application.Features.Printing.Printers.Queries;

public class GetAllPrintersQuery : IRequest<List<PrinterDto>>
{
}

