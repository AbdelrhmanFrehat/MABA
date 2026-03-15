using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Maba.Application.Features.Printing.PrintJobs.Commands;
using Maba.Application.Features.Printing.DTOs;
using Maba.Application.Features.Printing.PrintJobs.Queries;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PrintJobsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PrintJobsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<PrintJobDto>>> GetAllPrintJobs([FromQuery] Guid? printerId, [FromQuery] Guid? printJobStatusId)
    {
        var query = new GetAllPrintJobsQuery { PrinterId = printerId, PrintJobStatusId = printJobStatusId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<PrintJobDto>> CreatePrintJob([FromBody] CreatePrintJobCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAllPrintJobs), new { }, result);
    }

    [HttpGet("{id}/detail")]
    [AllowAnonymous]
    public async Task<ActionResult<PrintJobDetailDto>> GetPrintJobDetail(Guid id)
    {
        var query = new GetPrintJobDetailQuery { PrintJobId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<ActionResult<List<PrintJobDto>>> GetActivePrintJobs([FromQuery] Guid? printerId)
    {
        var query = new GetActivePrintJobsQuery { PrinterId = printerId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<PrintJobDto>> UpdatePrintJobStatus(Guid id, [FromBody] UpdatePrintJobStatusCommand command)
    {
        command.PrintJobId = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{id}/progress")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<PrintJobDto>> UpdatePrintJobProgress(Guid id, [FromBody] UpdatePrintJobProgressCommand command)
    {
        command.PrintJobId = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{id}/cancel")]
    [Authorize]
    public async Task<ActionResult<PrintJobDto>> CancelPrintJob(Guid id, [FromBody] CancelPrintJobCommand? command = null)
    {
        var cancelCommand = command ?? new CancelPrintJobCommand { PrintJobId = id };
        cancelCommand.PrintJobId = id;
        var result = await _mediator.Send(cancelCommand);
        return Ok(result);
    }
}

