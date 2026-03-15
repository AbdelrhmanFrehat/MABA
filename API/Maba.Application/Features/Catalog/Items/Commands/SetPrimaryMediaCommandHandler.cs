using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Media;

namespace Maba.Application.Features.Catalog.Items.Commands;

public class SetPrimaryMediaCommandHandler : IRequestHandler<SetPrimaryMediaCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public SetPrimaryMediaCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(SetPrimaryMediaCommand request, CancellationToken cancellationToken)
    {
        // Get all media links for this item
        var links = await _context.Set<EntityMediaLink>()
            .Where(eml => eml.EntityType == "Item" && eml.EntityId == request.ItemId)
            .ToListAsync(cancellationToken);

        var targetLink = links.FirstOrDefault(l => l.Id == request.MediaLinkId);
        
        if (targetLink == null)
        {
            throw new KeyNotFoundException("Media link not found");
        }

        // Unset all as primary
        foreach (var link in links)
        {
            link.IsPrimary = link.Id == request.MediaLinkId;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
