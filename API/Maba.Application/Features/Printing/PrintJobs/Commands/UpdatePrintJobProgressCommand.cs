using MediatR;
using Maba.Application.Features.Printing.DTOs;

namespace Maba.Application.Features.Printing.PrintJobs.Commands;

public class UpdatePrintJobProgressCommand : IRequest<PrintJobDto>
{
    public Guid PrintJobId { get; set; }
    public int ProgressPercent { get; set; }
    public DateTime? EstimatedCompletionTime { get; set; }
}

