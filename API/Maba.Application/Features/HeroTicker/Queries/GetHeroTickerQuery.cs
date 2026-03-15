using MediatR;
using Maba.Application.Features.HeroTicker.DTOs;

namespace Maba.Application.Features.HeroTicker.Queries;

/// <summary>
/// Public: active items ordered by SortOrder.
/// </summary>
public class GetHeroTickerQuery : IRequest<List<HeroTickerPublicDto>>
{
}
