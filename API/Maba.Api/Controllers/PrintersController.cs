using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Maba.Application.Features.Printing.Printers.Commands;
using Maba.Application.Features.Printing.DTOs;
using Maba.Application.Features.Printing.Printers.Queries;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PrintersController : ControllerBase
{
    private readonly IMediator _mediator;

    public PrintersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<PrinterDto>>> GetAllPrinters()
    {
        var query = new GetAllPrintersQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<PrinterDto>> CreatePrinter([FromBody] CreatePrinterCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAllPrinters), new { }, result);
    }
}

