using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maba.Application.Features.Finance.DTOs;
using Maba.Application.Features.Finance.Queries;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Admin,Manager,StoreOwner")]
public class FinanceController : ControllerBase
{
    private readonly IMediator _mediator;

    public FinanceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("kpis")]
    public async Task<ActionResult<FinanceKpisDto>> GetKpis()
    {
        var result = await _mediator.Send(new GetFinanceKpisQuery());
        return Ok(result);
    }

    [HttpGet("revenue-by-month")]
    public async Task<ActionResult<FinanceChartDto>> GetRevenueByMonth()
    {
        var result = await _mediator.Send(new GetFinanceRevenueByMonthQuery());
        return Ok(result);
    }

    [HttpGet("payment-methods-distribution")]
    public async Task<ActionResult<FinanceChartDto>> GetPaymentMethodsDistribution()
    {
        var result = await _mediator.Send(new GetFinancePaymentMethodsDistributionQuery());
        return Ok(result);
    }
}
