using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maba.Application.Features.Faq.Commands;
using Maba.Application.Features.Faq.DTOs;
using Maba.Application.Features.Faq.Queries;
using Maba.Domain.Faq;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/admin/faq")]
[Authorize(Roles = "Admin,StoreOwner")]
public class AdminFaqController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminFaqController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FaqItemDto>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetFaqItemByIdQuery { Id = id });
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<List<FaqItemDto>>> List(
        [FromQuery] string? search,
        [FromQuery] FaqCategory? category,
        [FromQuery] bool? isActive)
    {
        var result = await _mediator.Send(new GetFaqAdminQuery
        {
            Search = search,
            Category = category,
            IsActive = isActive
        });
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<FaqItemDto>> Create([FromBody] CreateFaqItemRequest request)
    {
        var result = await _mediator.Send(new CreateFaqItemCommand
        {
            Category = request.Category,
            QuestionEn = request.QuestionEn,
            QuestionAr = request.QuestionAr,
            AnswerEn = request.AnswerEn,
            AnswerAr = request.AnswerAr,
            IsActive = request.IsActive,
            IsFeatured = request.IsFeatured,
            SortOrder = request.SortOrder
        });
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateFaqItemRequest request)
    {
        if (id != request.Id) return BadRequest(new { message = "ID mismatch" });

        var success = await _mediator.Send(new UpdateFaqItemCommand
        {
            Id = id,
            Category = request.Category,
            QuestionEn = request.QuestionEn,
            QuestionAr = request.QuestionAr,
            AnswerEn = request.AnswerEn,
            AnswerAr = request.AnswerAr,
            IsActive = request.IsActive,
            IsFeatured = request.IsFeatured,
            SortOrder = request.SortOrder
        });

        if (!success) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var success = await _mediator.Send(new DeleteFaqItemCommand { Id = id });
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpPatch("{id:guid}/active")]
    public async Task<ActionResult> ToggleActive(Guid id)
    {
        var success = await _mediator.Send(new ToggleFaqItemActiveCommand { Id = id });
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpPatch("reorder")]
    public async Task<ActionResult> Reorder([FromBody] ReorderFaqRequest request)
    {
        await _mediator.Send(new ReorderFaqItemsCommand
        {
            Items = request.Items.Select(x => new FaqOrderItem { Id = x.Id, SortOrder = x.SortOrder }).ToList()
        });
        return NoContent();
    }
}

public class CreateFaqItemRequest
{
    public FaqCategory Category { get; set; }
    public string QuestionEn { get; set; } = string.Empty;
    public string? QuestionAr { get; set; }
    public string AnswerEn { get; set; } = string.Empty;
    public string? AnswerAr { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public int SortOrder { get; set; }
}

public class UpdateFaqItemRequest
{
    public Guid Id { get; set; }
    public FaqCategory Category { get; set; }
    public string QuestionEn { get; set; } = string.Empty;
    public string? QuestionAr { get; set; }
    public string AnswerEn { get; set; } = string.Empty;
    public string? AnswerAr { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public int SortOrder { get; set; }
}

public class ReorderFaqRequest
{
    public List<ReorderItem> Items { get; set; } = new();
}

public class ReorderItem
{
    public Guid Id { get; set; }
    public int SortOrder { get; set; }
}
