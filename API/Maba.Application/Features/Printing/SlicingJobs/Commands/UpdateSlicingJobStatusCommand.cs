using MediatR;
using Maba.Application.Features.Printing.DTOs;

namespace Maba.Application.Features.Printing.SlicingJobs.Commands;

public class UpdateSlicingJobStatusCommand : IRequest<SlicingJobDto>
{
    public Guid SlicingJobId { get; set; }
    public Guid SlicingJobStatusId { get; set; }
    public string? ErrorMessage { get; set; }
    public string? OutputFileUrl { get; set; }
    public decimal? EstimatedCost { get; set; }
}

