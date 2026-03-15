using MediatR;
using Maba.Application.Features.Printing.DTOs;

namespace Maba.Application.Features.Printing.PrintJobs.Commands;

public class CancelPrintJobCommand : IRequest<PrintJobDto>
{
    public Guid PrintJobId { get; set; }
    public string? Reason { get; set; }
}

