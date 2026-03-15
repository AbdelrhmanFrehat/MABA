using MediatR;
using Maba.Application.Features.Printing.DTOs;

namespace Maba.Application.Features.Printing.SlicingJobs.Queries;

public class GetSlicingJobDetailQuery : IRequest<SlicingJobDetailDto>
{
    public Guid SlicingJobId { get; set; }
}

