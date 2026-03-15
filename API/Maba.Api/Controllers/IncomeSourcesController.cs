using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Maba.Application.Features.Finance.DTOs;
using Maba.Application.Features.Finance.IncomeSources.Queries;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class IncomeSourcesController : ControllerBase
{
    private readonly IMediator _mediator;

    public IncomeSourcesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<IncomeSourceDto>>> GetAllIncomeSources()
    {
        var query = new GetAllIncomeSourcesQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}

