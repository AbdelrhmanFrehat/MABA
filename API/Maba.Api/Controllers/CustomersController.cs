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
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<CustomerDto>>> GetCustomers()
    {
        return Ok(await _mediator.Send(new GetCustomersQuery()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetCustomer(Guid id)
    {
        var result = await _mediator.Send(new GetCustomerByIdQuery { Id = id });
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> CreateCustomer([FromBody] CreateCustomerCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetCustomer), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CustomerDto>> UpdateCustomer(Guid id, [FromBody] UpdateCustomerCommand command)
    {
        command.Id = id;
        return Ok(await _mediator.Send(command));
    }
}
