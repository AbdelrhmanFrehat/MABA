using MediatR;
using Maba.Application.Features.Printing.DTOs;

namespace Maba.Application.Features.Printing.PrintJobs.Queries;

public class GetAllPrintJobsQuery : IRequest<List<PrintJobDto>>
{
    public Guid? PrinterId { get; set; }
    public Guid? PrintJobStatusId { get; set; }
}

