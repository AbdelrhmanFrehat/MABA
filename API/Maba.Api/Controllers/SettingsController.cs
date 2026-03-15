using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Maba.Application.Features.Common.Settings.Commands;
using Maba.Application.Features.Common.Settings.Queries;
using SystemSettingDto = Maba.Application.Features.Common.Settings.DTOs.SystemSettingDto;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<SystemSettingDto>>> GetSettings(
        [FromQuery] string? category,
        [FromQuery] bool? isPublic)
    {
        var query = new GetSystemSettingsQuery { Category = category, IsPublic = isPublic };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("key/{key}")]
    [AllowAnonymous]
    public async Task<ActionResult<SystemSettingDto>> GetSettingByKey(string key)
    {
        var query = new GetSystemSettingByKeyQuery { Key = key };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SystemSettingDto>> CreateSetting([FromBody] CreateSystemSettingCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetSettingByKey), new { key = result.Key }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SystemSettingDto>> UpdateSetting(Guid id, [FromBody] UpdateSystemSettingCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}

