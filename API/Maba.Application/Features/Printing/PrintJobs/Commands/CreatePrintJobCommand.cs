using MediatR;
using Maba.Application.Features.Printing.DTOs;

namespace Maba.Application.Features.Printing.PrintJobs.Commands;

public class CreatePrintJobCommand : IRequest<PrintJobDto>
{
    public Guid SlicingJobId { get; set; }
    public Guid PrinterId { get; set; }
}

