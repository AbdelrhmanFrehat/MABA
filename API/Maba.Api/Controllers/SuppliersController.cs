using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maba.Application.Features.Crm.Commands;
using Maba.Application.Features.Crm.DTOs;
using Maba.Application.Features.Crm.Queries;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly IMediator _mediator;

    public SuppliersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<SupplierDto>>> GetSuppliers()
    {
        return Ok(await _mediator.Send(new GetSuppliersQuery()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SupplierDto>> GetSupplier(Guid id)
    {
        var result = await _mediator.Send(new GetSupplierByIdQuery { Id = id });
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<SupplierDto>> CreateSupplier([FromBody] CreateSupplierCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetSupplier), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<SupplierDto>> UpdateSupplier(Guid id, [FromBody] UpdateSupplierCommand command)
    {
        command.Id = id;
        return Ok(await _mediator.Send(command));
    }
}
