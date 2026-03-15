using MediatR;

namespace Maba.Application.Features.Coupons.Commands;

public class DeleteCouponCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
