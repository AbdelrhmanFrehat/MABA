using MediatR;
using Maba.Application.Features.HeroTicker.DTOs;

namespace Maba.Application.Features.HeroTicker.Commands;

public class CreateHeroTickerItemCommand : IRequest<HeroTickerItemDto>
{
    public string? Title { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
