using MediatR;
using Maba.Application.Features.Printing.DTOs;

namespace Maba.Application.Features.Printing.PrintJobs.Commands;

public class UpdatePrintJobStatusCommand : IRequest<PrintJobDto>
{
    public Guid PrintJobId { get; set; }
    public Guid PrintJobStatusId { get; set; }
    public int? ProgressPercent { get; set; }
    public DateTime? EstimatedCompletionTime { get; set; }
    public string? ErrorMessage { get; set; }
}

