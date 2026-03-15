using MediatR;

namespace Maba.Application.Features.HeroTicker.Commands;

public class DeleteHeroTickerItemCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
