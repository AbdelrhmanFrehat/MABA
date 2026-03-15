using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Maba.Application.Features.HeroTicker.Commands;
using Maba.Application.Features.HeroTicker.DTOs;
using Maba.Application.Features.HeroTicker.Queries;
using System.Linq;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/hero-ticker")]
public class HeroTickerController : ControllerBase
{
    private readonly IMediator _mediator;

    public HeroTickerController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Public: active items ordered by SortOrder (for home hero ticker).
    /// </summary>
    [HttpGet]
    [Route("public")]
    [AllowAnonymous]
    public async Task<ActionResult<List<HeroTickerPublicDto>>> GetPublic()
    {
        var result = await _mediator.Send(new GetHeroTickerQuery());
        return Ok(result);
    }

    /// <summary>
    /// Admin: all items ordered by SortOrder.
    /// </summary>
    [HttpGet("admin")]
    [Authorize(Roles = "Admin,StoreOwner")]
    public async Task<ActionResult<List<HeroTickerItemDto>>> GetAdmin()
    {
        var result = await _mediator.Send(new GetHeroTickerAdminQuery());
        return Ok(result);
    }

    [HttpPost("admin")]
    [Authorize(Roles = "Admin,StoreOwner")]
    public async Task<ActionResult<HeroTickerItemDto>> Create([FromBody] CreateHeroTickerItemRequest request)
    {
        var result = await _mediator.Send(new CreateHeroTickerItemCommand
        {
            Title = request.Title,
            ImageUrl = request.ImageUrl,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive
        });
        return CreatedAtAction(nameof(GetAdmin), new { id = result.Id }, result);
    }

    [HttpPut("admin/{id:guid}")]
    [Authorize(Roles = "Admin,StoreOwner")]
    public async Task<ActionResult<HeroTickerItemDto>> Update(Guid id, [FromBody] UpdateHeroTickerItemRequest request)
    {
        if (id != request.Id) return BadRequest(new { message = "ID mismatch" });
        var result = await _mediator.Send(new UpdateHeroTickerItemCommand
        {
            Id = id,
            Title = request.Title,
            ImageUrl = request.ImageUrl,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive
        });
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("admin/{id:guid}")]
    [Authorize(Roles = "Admin,StoreOwner")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var found = await _mediator.Send(new DeleteHeroTickerItemCommand { Id = id });
        if (!found) return NotFound();
        return NoContent();
    }

    [HttpPatch("admin/reorder")]
    [Authorize(Roles = "Admin,StoreOwner")]
    public async Task<ActionResult> Reorder([FromBody] ReorderHeroTickerRequest request)
    {
        var items = request.Items.Select(x => new HeroTickerOrderItem { Id = x.Id, SortOrder = x.SortOrder }).ToList();
        await _mediator.Send(new ReorderHeroTickerItemsCommand { Items = items });
        return NoContent();
    }
}

public class CreateHeroTickerItemRequest
{
    public string? Title { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateHeroTickerItemRequest
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? ImageUrl { get; set; }
    public int? SortOrder { get; set; }
    public bool? IsActive { get; set; }
}

public class ReorderHeroTickerRequest
{
    public List<ReorderHeroTickerItemDto> Items { get; set; } = new();
}

public class ReorderHeroTickerItemDto
{
    public Guid Id { get; set; }
    public int SortOrder { get; set; }
}
