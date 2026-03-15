using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Maba.Application.Features.Catalog.Brands.Commands;
using Maba.Application.Features.Catalog.Brands.DTOs;
using Maba.Application.Features.Catalog.Brands.Queries;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class BrandsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BrandsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<BrandDto>>> GetAllBrands([FromQuery] bool? isActive)
    {
        var query = new GetAllBrandsQuery { IsActive = isActive };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<BrandDto>> GetBrandById(Guid id)
    {
        var query = new GetBrandByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,StoreOwner")]
    public async Task<ActionResult<BrandDto>> CreateBrand([FromBody] CreateBrandCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetBrandById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,StoreOwner")]
    public async Task<ActionResult<BrandDto>> UpdateBrand(Guid id, [FromBody] UpdateBrandCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,StoreOwner")]
    public async Task<ActionResult> DeleteBrand(Guid id)
    {
        var command = new DeleteBrandCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult<List<BrandDto>>> SearchBrands(
        [FromQuery] string? searchTerm,
        [FromQuery] bool? isActive)
    {
        var query = new SearchBrandsQuery { SearchTerm = searchTerm, IsActive = isActive };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}

