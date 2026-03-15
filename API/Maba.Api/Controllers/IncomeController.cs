using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Maba.Application.Features.Finance.Income.Commands;
using Maba.Application.Features.Finance.DTOs;
using Maba.Application.Features.Finance.Income.Queries;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class IncomeController : ControllerBase
{
    private readonly IMediator _mediator;

    public IncomeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<IncomeDto>>> GetAllIncome([FromQuery] Guid? incomeSourceId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var query = new GetAllIncomeQuery { IncomeSourceId = incomeSourceId, FromDate = fromDate, ToDate = toDate };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<IncomeDto>> CreateIncome([FromBody] CreateIncomeCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAllIncome), new { }, result);
    }
}

