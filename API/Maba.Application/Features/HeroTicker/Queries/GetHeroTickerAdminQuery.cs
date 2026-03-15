using MediatR;
using Maba.Application.Features.HeroTicker.DTOs;

namespace Maba.Application.Features.HeroTicker.Queries;

/// <summary>
/// Admin: all items ordered by SortOrder.
/// </summary>
public class GetHeroTickerAdminQuery : IRequest<List<HeroTickerItemDto>>
{
}
