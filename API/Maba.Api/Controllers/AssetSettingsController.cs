using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Maba.Application.Features.Assets.Commands;
using Maba.Application.Features.Assets.DTOs;
using Maba.Application.Features.Assets.Queries;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/asset-settings")]
[Authorize]
public class AssetSettingsController : ControllerBase
{
    private readonly IMediator _mediator;
    public AssetSettingsController(IMediator mediator) { _mediator = mediator; }

    [HttpGet("numbering")]
    public async Task<ActionResult<AssetNumberingSettingDto>> GetNumbering()
        => Ok(await _mediator.Send(new GetAssetNumberingSettingQuery()));

    [HttpPut("numbering")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AssetNumberingSettingDto>> UpdateNumbering([FromBody] UpdateAssetNumberingSettingCommand command)
        => Ok(await _mediator.Send(command));
}
