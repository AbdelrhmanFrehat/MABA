using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Maba.Application.Features.Common.Dashboard.Queries;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("Summary")]
    public async Task<ActionResult> GetSummary([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var query = new GetDashboardSummaryQuery
        {
            FromDate = fromDate,
            ToDate = toDate
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("SalesOverTime")]
    public async Task<ActionResult> GetSalesOverTime([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] int periods = 6)
    {
        var query = new GetSalesOverTimeQuery
        {
            FromDate = fromDate,
            ToDate = toDate,
            Periods = periods
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("OrdersByStatus")]
    public async Task<ActionResult> GetOrdersByStatus([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var query = new GetOrdersByStatusQuery
        {
            FromDate = fromDate,
            ToDate = toDate
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
