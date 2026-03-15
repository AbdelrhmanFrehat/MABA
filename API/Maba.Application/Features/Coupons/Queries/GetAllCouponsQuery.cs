using MediatR;
using Maba.Application.Features.Coupons.DTOs;

namespace Maba.Application.Features.Coupons.Queries;

public class GetAllCouponsQuery : IRequest<List<CouponDto>>
{
    public bool? ActiveOnly { get; set; }
}
