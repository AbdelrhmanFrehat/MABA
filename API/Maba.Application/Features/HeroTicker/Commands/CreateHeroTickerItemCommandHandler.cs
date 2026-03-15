using MediatR;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.HeroTicker.DTOs;
using Maba.Domain.HeroTicker;

namespace Maba.Application.Features.HeroTicker.Commands;

public class CreateHeroTickerItemCommandHandler : IRequestHandler<CreateHeroTickerItemCommand, HeroTickerItemDto>
{
    private readonly IApplicationDbContext _context;

    public CreateHeroTickerItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HeroTickerItemDto> Handle(CreateHeroTickerItemCommand request, CancellationToken cancellationToken)
    {
        var item = new HeroTickerItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            ImageUrl = request.ImageUrl ?? string.Empty,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive
        };

        _context.Set<HeroTickerItem>().Add(item);
        await _context.SaveChangesAsync(cancellationToken);

        return new HeroTickerItemDto
        {
            Id = item.Id,
            Title = item.Title,
            ImageUrl = item.ImageUrl,
            SortOrder = item.SortOrder,
            IsActive = item.IsActive,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        };
    }
}
