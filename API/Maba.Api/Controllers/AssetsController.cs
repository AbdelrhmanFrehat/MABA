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
public class AssetsController : ControllerBase
{
    private readonly IMediator _mediator;
    public AssetsController(IMediator mediator) { _mediator = mediator; }

    [HttpGet]
    public async Task<ActionResult<List<AssetDto>>> GetAll([FromQuery] Guid? categoryId, [FromQuery] Guid? investorUserId, [FromQuery] Guid? statusId, [FromQuery] string? search)
        => Ok(await _mediator.Send(new GetAssetsQuery { CategoryId = categoryId, InvestorUserId = investorUserId, StatusId = statusId, Search = search }));

    [HttpGet("{id}")]
    public async Task<ActionResult<AssetDto>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetAssetByIdQuery { Id = id });
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("by-number/{assetNumber}")]
    public async Task<ActionResult<AssetDto>> GetByNumber(string assetNumber)
    {
        var result = await _mediator.Send(new GetAssetByNumberQuery { AssetNumber = assetNumber });
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AssetDto>> Create([FromBody] CreateAssetCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AssetDto>> Update(Guid id, [FromBody] UpdateAssetCommand command)
    {
        command.Id = id;
        return Ok(await _mediator.Send(command));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteAssetCommand { Id = id });
        return NoContent();
    }
}
