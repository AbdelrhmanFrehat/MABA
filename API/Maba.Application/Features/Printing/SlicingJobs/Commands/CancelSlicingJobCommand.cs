using MediatR;
using Maba.Application.Features.Printing.DTOs;

namespace Maba.Application.Features.Printing.SlicingJobs.Commands;

public class CancelSlicingJobCommand : IRequest<SlicingJobDto>
{
    public Guid SlicingJobId { get; set; }
    public string? Reason { get; set; }
}

