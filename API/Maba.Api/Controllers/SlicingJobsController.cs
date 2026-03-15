using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Maba.Application.Features.Printing.SlicingJobs.Commands;
using Maba.Application.Features.Printing.DTOs;
using Maba.Application.Features.Printing.SlicingJobs.Queries;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class SlicingJobsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SlicingJobsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<SlicingJobDto>>> GetAllSlicingJobs([FromQuery] Guid? designFileId, [FromQuery] Guid? slicingJobStatusId)
    {
        var query = new GetAllSlicingJobsQuery { DesignFileId = designFileId, SlicingJobStatusId = slicingJobStatusId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<SlicingJobDto>> CreateSlicingJob([FromBody] CreateSlicingJobCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAllSlicingJobs), new { }, result);
    }

    [HttpGet("{id}/detail")]
    [AllowAnonymous]
    public async Task<ActionResult<SlicingJobDetailDto>> GetSlicingJobDetail(Guid id)
    {
        var query = new GetSlicingJobDetailQuery { SlicingJobId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<SlicingJobDto>> UpdateSlicingJobStatus(Guid id, [FromBody] UpdateSlicingJobStatusCommand command)
    {
        command.SlicingJobId = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{id}/cancel")]
    [Authorize]
    public async Task<ActionResult<SlicingJobDto>> CancelSlicingJob(Guid id, [FromBody] CancelSlicingJobCommand? command = null)
    {
        var cancelCommand = command ?? new CancelSlicingJobCommand { SlicingJobId = id };
        cancelCommand.SlicingJobId = id;
        var result = await _mediator.Send(cancelCommand);
        return Ok(result);
    }
}

