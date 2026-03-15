using MediatR;

namespace Maba.Application.Features.HeroTicker.Commands;

public class ReorderHeroTickerItemsCommand : IRequest<bool>
{
    public List<HeroTickerOrderItem> Items { get; set; } = new();
}

public class HeroTickerOrderItem
{
    public Guid Id { get; set; }
    public int SortOrder { get; set; }
}
