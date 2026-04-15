using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Maba.Application.Features.Assets.Commands;
using Maba.Application.Features.Assets.DTOs;
using Maba.Application.Features.Assets.Queries;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AssetCategoriesController : ControllerBase
{
    private readonly IMediator _mediator;
    public AssetCategoriesController(IMediator mediator) { _mediator = mediator; }

    [HttpGet]
    public async Task<ActionResult<List<AssetCategoryDto>>> GetAll()
        => Ok(await _mediator.Send(new GetAssetCategoriesQuery()));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AssetCategoryDto>> Create([FromBody] CreateAssetCategoryCommand command)
        => Ok(await _mediator.Send(command));

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AssetCategoryDto>> Update(Guid id, [FromBody] UpdateAssetCategoryCommand command)
    {
        command.Id = id;
        return Ok(await _mediator.Send(command));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteAssetCategoryCommand { Id = id });
        return NoContent();
    }
}
