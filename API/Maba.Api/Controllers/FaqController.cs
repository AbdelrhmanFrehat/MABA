using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maba.Application.Features.Faq.DTOs;
using Maba.Application.Features.Faq.Queries;
using Maba.Domain.Faq;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/faq")]
public class FaqController : ControllerBase
{
    private readonly IMediator _mediator;

    public FaqController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<FaqItemDto>>> GetFaq(
        [FromQuery] string? search,
        [FromQuery] FaqCategory? category,
        [FromQuery] bool? featured)
    {
        var result = await _mediator.Send(new GetFaqQuery
        {
            Search = search,
            Category = category,
            IsFeaturedOnly = featured
        });
        return Ok(result);
    }

    [HttpGet("categories")]
    [AllowAnonymous]
    public ActionResult<IEnumerable<object>> GetCategories()
    {
        var categories = Enum.GetValues<FaqCategory>()
            .Select(c => new { value = (int)c, name = c.ToString() });
        return Ok(categories);
    }
}
