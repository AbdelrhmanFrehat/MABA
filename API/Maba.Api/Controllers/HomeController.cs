using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Maba.Application.Features.Cms.DTOs;
using Maba.Application.Features.Cms.Pages.Queries;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[AllowAnonymous]
public class HomeController : ControllerBase
{
    private readonly IMediator _mediator;

    public HomeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Returns the published home page content. Same as GET /api/v1/pages/key/home.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PageDto>> GetHomePage()
    {
        try
        {
            var query = new GetPageByKeyQuery { Key = "home" };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return Ok(new PageDto
            {
                Key = "home",
                TitleEn = "Home",
                TitleAr = "الرئيسية",
                IsHome = true,
                IsActive = true,
                IsPublished = true
            });
        }
    }
}
