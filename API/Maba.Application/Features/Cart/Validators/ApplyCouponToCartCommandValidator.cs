using FluentValidation;
using Maba.Application.Features.Cart.Commands;

namespace Maba.Application.Features.Cart.Validators;

public class ApplyCouponToCartCommandValidator : AbstractValidator<ApplyCouponToCartCommand>
{
    public ApplyCouponToCartCommandValidator()
    {
        RuleFor(x => x.CouponCode)
            .NotEmpty().WithMessage("Coupon code is required")
            .MaximumLength(50).WithMessage("Coupon code cannot exceed 50 characters");

        RuleFor(x => x)
            .Must(x => x.UserId.HasValue || !string.IsNullOrEmpty(x.SessionId))
            .WithMessage("Either user ID or session ID is required");
    }
}
