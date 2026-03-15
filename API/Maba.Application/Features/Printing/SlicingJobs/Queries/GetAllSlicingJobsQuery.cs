using MediatR;
using Maba.Application.Features.Printing.DTOs;

namespace Maba.Application.Features.Printing.SlicingJobs.Queries;

public class GetAllSlicingJobsQuery : IRequest<List<SlicingJobDto>>
{
    public Guid? DesignFileId { get; set; }
    public Guid? SlicingJobStatusId { get; set; }
}

