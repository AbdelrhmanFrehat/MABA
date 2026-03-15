using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Maba.Application.Features.Catalog.Items.Commands;
using Maba.Application.Features.Catalog.Items.DTOs;
using Maba.Application.Features.Catalog.Items.Queries;
using Maba.Application.Common.Models;
using Maba.Application.Features.Machines.Queries;
using MachineDto = Maba.Application.Features.Machines.DTOs.MachineDto;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ItemsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ItemsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<ItemDto>>> GetAllItems(
        [FromQuery] Guid? categoryId,
        [FromQuery] Guid? brandId,
        [FromQuery] Guid? itemStatusId,
        [FromQuery] Guid? tagId,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice)
    {
        var query = new GetAllItemsQuery
        {
            CategoryId = categoryId,
            BrandId = brandId,
            ItemStatusId = itemStatusId,
            TagId = tagId,
            MinPrice = minPrice,
            MaxPrice = maxPrice
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ItemDto>> GetItemById(Guid id)
    {
        var query = new GetItemByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,StoreOwner")]
    public async Task<ActionResult<ItemDto>> CreateItem([FromBody] CreateItemCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetItemById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,StoreOwner")]
    public async Task<ActionResult<ItemDto>> UpdateItem(Guid id, [FromBody] UpdateItemCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,StoreOwner")]
    public async Task<ActionResult> DeleteItem(Guid id)
    {
        var command = new DeleteItemCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<ItemDto>>> SearchItems(
        [FromQuery] string? searchTerm,
        [FromQuery] Guid? categoryId,
        [FromQuery] Guid? brandId,
        [FromQuery] Guid? itemStatusId,
        [FromQuery] Guid? tagId,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] bool? isFeatured,
        [FromQuery] bool? isNew,
        [FromQuery] bool? isOnSale,
        [FromQuery] bool? inStock,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = true)
    {
        var query = new SearchItemsQuery
        {
            SearchTerm = searchTerm,
            CategoryId = categoryId,
            BrandId = brandId,
            ItemStatusId = itemStatusId,
            TagId = tagId,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            IsFeatured = isFeatured,
            IsNew = isNew,
            IsOnSale = isOnSale,
            InStock = inStock,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDescending = sortDescending
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("statuses")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ItemStatusDto>>> GetAllItemStatuses()
    {
        var query = new GetAllItemStatusesQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}/detail")]
    [AllowAnonymous]
    public async Task<ActionResult<ItemDetailDto>> GetItemDetail(Guid id)
    {
        var query = new GetItemDetailQuery { ItemId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("featured")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ItemDto>>> GetFeaturedItems()
    {
        var query = new GetFeaturedItemsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("new")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ItemDto>>> GetNewItems()
    {
        var query = new GetNewItemsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("sale")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ItemDto>>> GetOnSaleItems()
    {
        var query = new GetOnSaleItemsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}/related")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ItemDto>>> GetRelatedItems(Guid id)
    {
        var query = new GetRelatedItemsQuery { ItemId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("{id}/reviews")]
    [Authorize]
    public async Task<ActionResult<ReviewDto>> CreateReview(Guid id, [FromBody] CreateReviewCommand command)
    {
        command.ItemId = id;
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetItemReviews), new { id }, result);
    }

    [HttpGet("{id}/reviews")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ReviewDto>>> GetItemReviews(Guid id, [FromQuery] bool? isApproved)
    {
        var query = new GetItemReviewsQuery { ItemId = id, IsApproved = isApproved };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPut("{id}/reviews/{reviewId}")]
    [Authorize]
    public async Task<ActionResult<ReviewDto>> UpdateReview(Guid id, Guid reviewId, [FromBody] UpdateReviewCommand command)
    {
        command.ReviewId = reviewId;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}/reviews/{reviewId}")]
    [Authorize]
    public async Task<ActionResult> DeleteReview(Guid id, Guid reviewId)
    {
        var command = new DeleteReviewCommand { ReviewId = reviewId };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPost("{id}/reviews/{reviewId}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ReviewDto>> ApproveReview(Guid id, Guid reviewId)
    {
        var command = new ApproveReviewCommand { ReviewId = reviewId };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("{id}/comments")]
    [Authorize]
    public async Task<ActionResult<CommentDto>> CreateComment(Guid id, [FromBody] CreateCommentCommand command)
    {
        command.ItemId = id;
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetItemComments), new { id }, result);
    }

    [HttpGet("{id}/comments")]
    [AllowAnonymous]
    public async Task<ActionResult<List<CommentDto>>> GetItemComments(Guid id, [FromQuery] bool? isApproved)
    {
        var query = new GetItemCommentsQuery { ItemId = id, IsApproved = isApproved };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPut("{id}/comments/{commentId}")]
    [Authorize]
    public async Task<ActionResult<CommentDto>> UpdateComment(Guid id, Guid commentId, [FromBody] UpdateCommentCommand command)
    {
        command.CommentId = commentId;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}/comments/{commentId}")]
    [Authorize]
    public async Task<ActionResult> DeleteComment(Guid id, Guid commentId)
    {
        var command = new DeleteCommentCommand { CommentId = commentId };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPost("{id}/comments/{commentId}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<CommentDto>> ApproveComment(Guid id, Guid commentId)
    {
        var command = new ApproveCommentCommand { CommentId = commentId };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("{id}/machines")]
    [AllowAnonymous]
    public async Task<ActionResult<List<MachineDto>>> GetItemMachines(Guid id)
    {
        var query = new GetMachinesByItemQuery { ItemId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    // ============ ITEM MEDIA MANAGEMENT ============

    [HttpGet("{id}/media")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ItemMediaDto>>> GetItemMedia(Guid id)
    {
        var query = new GetItemMediaQuery { ItemId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("{id}/media")]
    [Authorize(Roles = "Admin,StoreOwner")]
    public async Task<ActionResult<ItemMediaDto>> LinkMediaToItem(Guid id, [FromBody] LinkMediaRequest request)
    {
        var command = new LinkMediaToItemCommand
        {
            ItemId = id,
            MediaAssetId = request.MediaAssetId,
            IsPrimary = request.IsPrimary,
            SortOrder = request.SortOrder
        };
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetItemMedia), new { id }, result);
    }

    [HttpDelete("{id}/media/{mediaLinkId}")]
    [Authorize(Roles = "Admin,StoreOwner")]
    public async Task<ActionResult> UnlinkMediaFromItem(Guid id, Guid mediaLinkId)
    {
        var command = new UnlinkMediaFromItemCommand
        {
            ItemId = id,
            MediaLinkId = mediaLinkId
        };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPut("{id}/media/{mediaLinkId}/primary")]
    [Authorize(Roles = "Admin,StoreOwner")]
    public async Task<ActionResult> SetPrimaryMedia(Guid id, Guid mediaLinkId)
    {
        var command = new SetPrimaryMediaCommand
        {
            ItemId = id,
            MediaLinkId = mediaLinkId
        };
        await _mediator.Send(command);
        return NoContent();
    }
}

public class LinkMediaRequest
{
    public Guid MediaAssetId { get; set; }
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
}

