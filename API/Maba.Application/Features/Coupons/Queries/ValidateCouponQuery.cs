using MediatR;
using Maba.Application.Features.Coupons.DTOs;

namespace Maba.Application.Features.Coupons.Queries;

public class ValidateCouponQuery : IRequest<ValidateCouponResultDto>
{
    public string Code { get; set; } = string.Empty;
    public decimal OrderTotal { get; set; }
    public Guid? UserId { get; set; }
}
