using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maba.Application.Features.Catalog.Items.Commands;
using Maba.Application.Features.Catalog.Items.DTOs;
using Maba.Application.Features.Catalog.Items.Queries;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Admin,Manager,StoreOwner")]
public class ReviewsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReviewsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Admin list with optional status/rating filters (route name is legacy: not limited to pending).</summary>
    [HttpGet("pending")]
    public async Task<ActionResult<AdminReviewListResponse>> GetReviewsForAdmin(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] int? rating = null)
    {
        var query = new GetAdminReviewsQuery
        {
            Page = page,
            PageSize = pageSize,
            Status = status,
            Rating = rating
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<ReviewDto>> Approve(Guid id)
    {
        var result = await _mediator.Send(new ApproveReviewCommand { ReviewId = id });
        return Ok(result);
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<ActionResult<ReviewDto>> Reject(Guid id, [FromBody] RejectReviewRequestBody? body)
    {
        var result = await _mediator.Send(new RejectReviewCommand { ReviewId = id, Reason = body?.Reason });
        return Ok(result);
    }
}

public class RejectReviewRequestBody
{
    public string? Reason { get; set; }
}
