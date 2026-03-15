using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Maba.Application.Features.Finance.Expenses.Commands;
using Maba.Application.Features.Finance.DTOs;
using Maba.Application.Features.Finance.Expenses.Queries;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ExpensesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExpensesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<ExpenseDto>>> GetAllExpenses([FromQuery] Guid? expenseCategoryId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var query = new GetAllExpensesQuery { ExpenseCategoryId = expenseCategoryId, FromDate = fromDate, ToDate = toDate };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseDto>> CreateExpense([FromBody] CreateExpenseCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAllExpenses), new { }, result);
    }
}

