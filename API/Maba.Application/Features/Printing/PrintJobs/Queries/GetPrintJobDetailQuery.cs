using MediatR;
using Maba.Application.Features.Printing.DTOs;

namespace Maba.Application.Features.Printing.PrintJobs.Queries;

public class GetPrintJobDetailQuery : IRequest<PrintJobDetailDto>
{
    public Guid PrintJobId { get; set; }
}

