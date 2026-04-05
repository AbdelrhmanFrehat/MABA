using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maba.Application.Features.Lookups.Commands;
using Maba.Application.Features.Lookups.DTOs;
using Maba.Application.Features.Lookups.Queries;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class LookupsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LookupsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<LookupTypeDto>>> GetLookupTypes()
    {
        return Ok(await _mediator.Send(new GetLookupTypesQuery()));
    }

    [HttpGet("{typeKey}/values")]
    public async Task<ActionResult<List<LookupValueDto>>> GetLookupValues(string typeKey)
    {
        return Ok(await _mediator.Send(new GetLookupValuesByTypeQuery { TypeKey = typeKey }));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<LookupTypeDto>> CreateLookupType([FromBody] CreateLookupTypeCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetLookupTypes), new { id = result.Id }, result);
    }

    [HttpPost("values")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<LookupValueDto>> CreateLookupValue([FromBody] CreateLookupValueCommand command)
    {
        return Ok(await _mediator.Send(command));
    }
}
