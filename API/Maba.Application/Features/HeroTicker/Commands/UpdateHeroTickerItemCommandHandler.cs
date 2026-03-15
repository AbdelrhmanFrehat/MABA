using MediatR;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.HeroTicker.DTOs;
using Maba.Domain.HeroTicker;

namespace Maba.Application.Features.HeroTicker.Commands;

public class UpdateHeroTickerItemCommandHandler : IRequestHandler<UpdateHeroTickerItemCommand, HeroTickerItemDto?>
{
    private readonly IApplicationDbContext _context;

    public UpdateHeroTickerItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HeroTickerItemDto?> Handle(UpdateHeroTickerItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.Set<HeroTickerItem>().FindAsync(new object[] { request.Id }, cancellationToken);
        if (item == null) return null;

        if (request.Title != null) item.Title = request.Title;
        if (request.ImageUrl != null) item.ImageUrl = request.ImageUrl;
        if (request.SortOrder.HasValue) item.SortOrder = request.SortOrder.Value;
        if (request.IsActive.HasValue) item.IsActive = request.IsActive.Value;
        item.UpdatedAt = DateTime.UtcNow;

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
