using MediatR;
using Maba.Application.Features.Printing.DTOs;

namespace Maba.Application.Features.Printing.SlicingJobs.Commands;

public class CreateSlicingJobCommand : IRequest<SlicingJobDto>
{
    public Guid DesignFileId { get; set; }
    public Guid SlicingProfileId { get; set; }
}

