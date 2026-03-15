using MediatR;
using Maba.Application.Features.Printing.DTOs;

namespace Maba.Application.Features.Printing.PrintJobs.Queries;

public class GetActivePrintJobsQuery : IRequest<List<PrintJobDto>>
{
    public Guid? PrinterId { get; set; }
}

