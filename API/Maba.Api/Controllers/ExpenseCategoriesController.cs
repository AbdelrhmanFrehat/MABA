using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Maba.Application.Features.Finance.DTOs;
using Maba.Application.Features.Finance.ExpenseCategories.Queries;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ExpenseCategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExpenseCategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<ExpenseCategoryDto>>> GetAllExpenseCategories()
    {
        var query = new GetAllExpenseCategoriesQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}

