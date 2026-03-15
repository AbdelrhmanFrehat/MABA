using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Maba.Application.Features.Coupons.Commands;
using Maba.Application.Features.Coupons.Queries;
using Maba.Application.Features.Coupons.DTOs;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/coupons")]
public class CouponsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CouponsController> _logger;

    public CouponsController(IMediator mediator, ILogger<CouponsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Validate a coupon code (public endpoint)
    /// </summary>
    [HttpPost("validate")]
    [AllowAnonymous]
    public async Task<ActionResult<ValidateCouponResultDto>> ValidateCoupon(
        [FromBody] ValidateCouponRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return BadRequest("Coupon code is required.");
        }

        var query = new ValidateCouponQuery
        {
            Code = request.Code,
            OrderTotal = request.OrderTotal,
            UserId = request.UserId
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get all coupons (admin only)
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<CouponDto>>> GetAllCoupons(
        [FromQuery] bool? activeOnly = null)
    {
        var query = new GetAllCouponsQuery { ActiveOnly = activeOnly };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Create a new coupon (admin only)
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<CouponDto>> CreateCoupon(
        [FromBody] CreateCouponCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Code))
        {
            return BadRequest("Coupon code is required.");
        }
        if (command.Value <= 0)
        {
            return BadRequest("Coupon value must be greater than zero.");
        }

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update a coupon (admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult> UpdateCoupon(
        Guid id,
        [FromBody] UpdateCouponCommand command)
    {
        command.Id = id;
        
        try
        {
            var result = await _mediator.Send(command);
            if (!result)
            {
                return NotFound();
            }
            return Ok(new { success = true, message = "Coupon updated successfully" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Delete a coupon (admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> DeleteCoupon(Guid id)
    {
        var command = new DeleteCouponCommand { Id = id };
        var result = await _mediator.Send(command);

        if (!result)
        {
            return NotFound();
        }

        return Ok(new { success = true, message = "Coupon deleted successfully" });
    }
}

public class ValidateCouponRequest
{
    public string Code { get; set; } = string.Empty;
    public decimal OrderTotal { get; set; }
    public Guid? UserId { get; set; }
}
