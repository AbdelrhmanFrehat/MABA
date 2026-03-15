using MediatR;
using Maba.Application.Features.HeroTicker.DTOs;

namespace Maba.Application.Features.HeroTicker.Commands;

public class UpdateHeroTickerItemCommand : IRequest<HeroTickerItemDto?>
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? ImageUrl { get; set; }
    public int? SortOrder { get; set; }
    public bool? IsActive { get; set; }
}
