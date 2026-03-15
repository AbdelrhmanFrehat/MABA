using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Maba.Application.Features.Cart.Commands;
using Maba.Application.Features.Cart.DTOs;
using Maba.Application.Features.Cart.Queries;
using Maba.Application.Features.Orders.DTOs;
using System.Security.Claims;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CartController : ControllerBase
{
    private readonly IMediator _mediator;

    public CartController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private string? GetSessionId()
    {
        return Request.Headers["X-Session-Id"].FirstOrDefault();
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<CartDto>> GetCart()
    {
        var userId = GetCurrentUserId();
        var sessionId = GetSessionId();

        var query = new GetCartQuery
        {
            UserId = userId,
            SessionId = sessionId
        };

        var result = await _mediator.Send(query);
        
        if (result == null)
        {
            // Return empty cart
            return Ok(new CartDto
            {
                Items = new List<CartItemDto>(),
                Subtotal = 0,
                TaxAmount = 0,
                ShippingAmount = 0,
                DiscountAmount = 0,
                Total = 0,
                Currency = "ILS"
            });
        }

        return Ok(result);
    }

    [HttpPost("items")]
    [AllowAnonymous]
    public async Task<ActionResult<CartDto>> AddToCart([FromBody] AddToCartRequest request)
    {
        var userId = GetCurrentUserId();
        var sessionId = GetSessionId();

        if (!userId.HasValue && string.IsNullOrEmpty(sessionId))
        {
            return BadRequest("Either authentication or session ID is required");
        }

        var command = new AddToCartCommand
        {
            UserId = userId,
            SessionId = sessionId,
            ItemId = request.ItemId,
            Quantity = request.Quantity
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("items/{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<CartDto>> UpdateCartItem(Guid id, [FromBody] UpdateCartItemRequest request)
    {
        var userId = GetCurrentUserId();
        var sessionId = GetSessionId();

        var command = new UpdateCartItemCommand
        {
            UserId = userId,
            SessionId = sessionId,
            CartItemId = id,
            Quantity = request.Quantity
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("items/{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<CartDto>> RemoveCartItem(Guid id)
    {
        var userId = GetCurrentUserId();
        var sessionId = GetSessionId();

        var command = new RemoveCartItemCommand
        {
            UserId = userId,
            SessionId = sessionId,
            CartItemId = id
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete]
    [AllowAnonymous]
    public async Task<ActionResult> ClearCart()
    {
        var userId = GetCurrentUserId();
        var sessionId = GetSessionId();

        var command = new ClearCartCommand
        {
            UserId = userId,
            SessionId = sessionId
        };

        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPost("merge")]
    [Authorize]
    public async Task<ActionResult<CartDto>> MergeGuestCart([FromBody] MergeCartRequest request)
    {
        var userId = GetCurrentUserId();
        
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var command = new MergeGuestCartCommand
        {
            UserId = userId.Value,
            SessionId = request.SessionId
        };

        var result = await _mediator.Send(command);
        return Ok(result ?? new CartDto());
    }

    [HttpPost("checkout")]
    [Authorize]
    public async Task<ActionResult<OrderDto>> Checkout([FromBody] CheckoutRequest request)
    {
        var userId = GetCurrentUserId();
        var sessionId = GetSessionId();

        if (!userId.HasValue && string.IsNullOrEmpty(sessionId))
        {
            return Unauthorized("Authentication required for checkout");
        }

        var command = new CheckoutCartCommand
        {
            UserId = userId ?? Guid.Empty,
            SessionId = sessionId,
            ShippingAddressJson = System.Text.Json.JsonSerializer.Serialize(request.ShippingAddress),
            BillingAddressJson = System.Text.Json.JsonSerializer.Serialize(request.BillingAddress),
            PaymentMethod = request.PaymentMethod,
            ShippingMethod = request.ShippingMethod,
            Notes = request.Notes
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetCart), result);
    }
}

// Request DTOs
public class AddToCartRequest
{
    public Guid ItemId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class UpdateCartItemRequest
{
    public int Quantity { get; set; }
}

public class MergeCartRequest
{
    public string SessionId { get; set; } = string.Empty;
}

public class CheckoutRequest
{
    public ShippingAddressDto ShippingAddress { get; set; } = new();
    public BillingAddressDto BillingAddress { get; set; } = new();
    public string PaymentMethod { get; set; } = "Cash";
    public string ShippingMethod { get; set; } = "Standard";
    public string? Notes { get; set; }
}

public class ShippingAddressDto
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string Country { get; set; } = string.Empty;
}

public class BillingAddressDto
{
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string Country { get; set; } = string.Empty;
}
