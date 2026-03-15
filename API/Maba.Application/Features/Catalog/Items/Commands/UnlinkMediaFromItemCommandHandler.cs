using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Media;

namespace Maba.Application.Features.Catalog.Items.Commands;

public class UnlinkMediaFromItemCommandHandler : IRequestHandler<UnlinkMediaFromItemCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public UnlinkMediaFromItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UnlinkMediaFromItemCommand request, CancellationToken cancellationToken)
    {
        var link = await _context.Set<EntityMediaLink>()
            .FirstOrDefaultAsync(eml => 
                eml.Id == request.MediaLinkId && 
                eml.EntityType == "Item" && 
                eml.EntityId == request.ItemId, cancellationToken);

        if (link == null)
        {
            throw new KeyNotFoundException("Media link not found");
        }

        _context.Set<EntityMediaLink>().Remove(link);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
