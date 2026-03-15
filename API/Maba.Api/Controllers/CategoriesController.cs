using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Maba.Application.Features.Catalog.Categories.Commands;
using Maba.Application.Features.Catalog.Categories.DTOs;
using Maba.Application.Features.Catalog.Categories.Queries;
using CategoryTreeDto = Maba.Application.Features.Catalog.Categories.Queries.CategoryTreeDto;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<CategoryDto>>> GetAllCategories([FromQuery] bool? isActive, [FromQuery] bool includeChildren = false)
    {
        var query = new GetAllCategoriesQuery { IsActive = isActive, IncludeChildren = includeChildren };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<CategoryDto>> GetCategoryById(Guid id)
    {
        var query = new GetCategoryByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,StoreOwner")]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetCategoryById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,StoreOwner")]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(Guid id, [FromBody] UpdateCategoryCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,StoreOwner")]
    public async Task<ActionResult> DeleteCategory(Guid id)
    {
        var command = new DeleteCategoryCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpGet("tree")]
    [AllowAnonymous]
    public async Task<ActionResult<List<CategoryTreeDto>>> GetCategoryTree()
    {
        var query = new GetCategoryTreeQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("parent/{parentId?}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<CategoryDto>>> GetCategoriesByParent(Guid? parentId = null)
    {
        var query = new GetCategoriesByParentQuery { ParentId = parentId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}

