using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Items.DTOs;
using Maba.Application.Features.Catalog.Items.Queries;
using Maba.Domain.Catalog;

namespace Maba.Application.Features.Catalog.Items.Handlers;

public class GetItemByIdQueryHandler : IRequestHandler<GetItemByIdQuery, ItemDto>
{
    private readonly IApplicationDbContext _context;

    public GetItemByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ItemDto> Handle(GetItemByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await _context.Set<Item>()
            .Include(i => i.ItemStatus)
            .Include(i => i.Brand)
            .Include(i => i.Category)
            .Include(i => i.ItemTags)
            .ThenInclude(it => it.Tag)
            .Include(i => i.Inventory)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (item == null)
        {
            throw new KeyNotFoundException("Item not found");
        }

        // Increment views count
        item.ViewsCount++;
        await _context.SaveChangesAsync(cancellationToken);

        return CreateItemCommandHandler.MapToDto(item);
    }
}

