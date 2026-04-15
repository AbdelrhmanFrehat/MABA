using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Wishlist;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class WishlistController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public WishlistController(IApplicationDbContext context)
    {
        _context = context;
    }

    private Guid? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    // ─────────────────────────────────────────────
    // GET /api/v1/wishlist
    // ─────────────────────────────────────────────
    [HttpGet]
    public async Task<ActionResult<WishlistDto>> GetWishlist(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var wishlist = await GetOrCreateWishlistAsync(userId.Value, ct);
        return Ok(MapToDto(wishlist));
    }

    // ─────────────────────────────────────────────
    // POST /api/v1/wishlist/items
    // ─────────────────────────────────────────────
    [HttpPost("items")]
    public async Task<ActionResult<WishlistDto>> AddItem([FromBody] AddToWishlistRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var wishlist = await GetOrCreateWishlistAsync(userId.Value, ct);

        // Skip if already in wishlist
        bool alreadyExists = wishlist.Items.Any(i => i.ItemId == request.ItemId);
        if (!alreadyExists)
        {
            var item = new WishlistItem
            {
                WishlistId = wishlist.Id,
                ItemId = request.ItemId,
                AddedAt = DateTime.UtcNow
            };
            _context.Set<WishlistItem>().Add(item);
            await _context.SaveChangesAsync(ct);

            // Reload to include item navigation
            wishlist = await LoadWishlistAsync(userId.Value, ct) ?? wishlist;
        }

        return Ok(MapToDto(wishlist));
    }

    // ─────────────────────────────────────────────
    // DELETE /api/v1/wishlist/items/{itemId}
    // ─────────────────────────────────────────────
    [HttpDelete("items/{itemId:guid}")]
    public async Task<ActionResult<WishlistDto>> RemoveItem(Guid itemId, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var wishlist = await LoadWishlistAsync(userId.Value, ct);
        if (wishlist == null) return Ok(new WishlistDto { UserId = userId.Value.ToString(), Items = [] });

        // itemId can be either the WishlistItem.Id or the Item.Id
        var entry = wishlist.Items.FirstOrDefault(i => i.Id == itemId || i.ItemId == itemId);
        if (entry != null)
        {
            _context.Set<WishlistItem>().Remove(entry);
            await _context.SaveChangesAsync(ct);
            wishlist.Items.Remove(entry);
        }

        return Ok(MapToDto(wishlist));
    }

    // ─────────────────────────────────────────────
    // DELETE /api/v1/wishlist/clear
    // ─────────────────────────────────────────────
    [HttpDelete("clear")]
    public async Task<ActionResult> ClearWishlist(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var wishlist = await LoadWishlistAsync(userId.Value, ct);
        if (wishlist != null && wishlist.Items.Any())
        {
            _context.Set<WishlistItem>().RemoveRange(wishlist.Items);
            await _context.SaveChangesAsync(ct);
        }

        return NoContent();
    }

    // ─────────────────────────────────────────────
    // GET /api/v1/wishlist/items/{itemId}/check
    // ─────────────────────────────────────────────
    [HttpGet("items/{itemId:guid}/check")]
    public async Task<ActionResult<bool>> CheckItem(Guid itemId, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Ok(false);

        var exists = await _context.Set<Wishlist>()
            .Where(w => w.UserId == userId.Value)
            .SelectMany(w => w.Items)
            .AnyAsync(i => i.ItemId == itemId, ct);

        return Ok(exists);
    }

    // ─────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────

    private async Task<Domain.Wishlist.Wishlist> GetOrCreateWishlistAsync(Guid userId, CancellationToken ct)
    {
        var existing = await LoadWishlistAsync(userId, ct);
        if (existing != null) return existing;

        var wishlist = new Domain.Wishlist.Wishlist { UserId = userId };
        _context.Set<Domain.Wishlist.Wishlist>().Add(wishlist);
        await _context.SaveChangesAsync(ct);
        wishlist.Items = new List<WishlistItem>();
        return wishlist;
    }

    private async Task<Domain.Wishlist.Wishlist?> LoadWishlistAsync(Guid userId, CancellationToken ct)
    {
        return await _context.Set<Domain.Wishlist.Wishlist>()
            .Include(w => w.Items)
                .ThenInclude(i => i.Item)
            .FirstOrDefaultAsync(w => w.UserId == userId, ct);
    }

    private static WishlistDto MapToDto(Domain.Wishlist.Wishlist wishlist)
    {
        return new WishlistDto
        {
            Id = wishlist.Id.ToString(),
            UserId = wishlist.UserId.ToString(),
            CreatedAt = wishlist.CreatedAt.ToString("O"),
            UpdatedAt = wishlist.UpdatedAt?.ToString("O"),
            Items = wishlist.Items.Select(i => new WishlistItemDto
            {
                Id = i.Id.ToString(),
                WishlistId = i.WishlistId.ToString(),
                ItemId = i.ItemId.ToString(),
                AddedAt = i.AddedAt.ToString("O"),
                Item = i.Item == null ? null : new WishlistItemDetailDto
                {
                    Id = i.Item.Id.ToString(),
                    NameEn = i.Item.NameEn,
                    NameAr = i.Item.NameAr,
                    Sku = i.Item.Sku,
                    Price = i.Item.Price,
                    Currency = i.Item.Currency,
                    AverageRating = (double)i.Item.AverageRating,
                    ReviewsCount = i.Item.ReviewsCount
                }
            }).ToList()
        };
    }
}

// ─────────────────────────────────────────────
// DTOs
// ─────────────────────────────────────────────

public class WishlistDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public List<WishlistItemDto> Items { get; set; } = [];
    public string? CreatedAt { get; set; }
    public string? UpdatedAt { get; set; }
}

public class WishlistItemDto
{
    public string Id { get; set; } = string.Empty;
    public string WishlistId { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty;
    public WishlistItemDetailDto? Item { get; set; }
    public string AddedAt { get; set; } = string.Empty;
}

public class WishlistItemDetailDto
{
    public string Id { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int ReviewsCount { get; set; }
}

public class AddToWishlistRequest
{
    public Guid ItemId { get; set; }
}
