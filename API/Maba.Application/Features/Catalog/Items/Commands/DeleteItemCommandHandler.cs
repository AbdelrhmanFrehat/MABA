using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Items.Commands;
using Maba.Domain.Catalog;
using Maba.Domain.Machines;
using Maba.Domain.Orders;
using DomainInventory = Maba.Domain.Catalog.Inventory;

namespace Maba.Application.Features.Catalog.Items.Handlers;

public class DeleteItemCommandHandler : IRequestHandler<DeleteItemCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.Set<Item>()
            .Include(i => i.ItemTags)
            .Include(i => i.ItemSections)
            .Include(i => i.Reviews)
            .Include(i => i.Comments)
            .Include(i => i.Inventory)
            .Include(i => i.ItemMachineLinks)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (item == null)
        {
            throw new KeyNotFoundException("Item not found");
        }

        // Remove related entities
        foreach (var itemTag in item.ItemTags.ToList())
        {
            _context.Set<ItemTag>().Remove(itemTag);
        }

        foreach (var section in item.ItemSections.ToList())
        {
            var features = await _context.Set<ItemSectionFeature>()
                .Where(f => f.ItemSectionId == section.Id)
                .ToListAsync(cancellationToken);
            foreach (var feature in features)
            {
                _context.Set<ItemSectionFeature>().Remove(feature);
            }
            _context.Set<ItemSection>().Remove(section);
        }

        if (item.Inventory != null)
        {
            _context.Set<DomainInventory>().Remove(item.Inventory);
        }

        foreach (var link in item.ItemMachineLinks.ToList())
        {
            _context.Set<ItemMachineLink>().Remove(link);
        }

        // Keep historical orders but detach this item to avoid FK conflicts on delete.
        var orderItems = await _context.Set<OrderItem>()
            .Where(oi => oi.ItemId == item.Id)
            .ToListAsync(cancellationToken);
        foreach (var orderItem in orderItems)
        {
            orderItem.ItemId = null;
        }

        // Note: Reviews and Comments might be kept for historical purposes
        // If you want to delete them, uncomment the following:
        // foreach (var review in item.Reviews.ToList())
        // {
        //     _context.Set<Review>().Remove(review);
        // }
        // foreach (var comment in item.Comments.ToList())
        // {
        //     _context.Set<Comment>().Remove(comment);
        // }

        _context.Set<Item>().Remove(item);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

