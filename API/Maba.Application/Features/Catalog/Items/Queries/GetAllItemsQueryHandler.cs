using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Items.DTOs;
using Maba.Application.Features.Catalog.Items.Queries;
using Maba.Domain.Catalog;
using Maba.Domain.Media;

namespace Maba.Application.Features.Catalog.Items.Handlers;

public class GetAllItemsQueryHandler : IRequestHandler<GetAllItemsQuery, List<ItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllItemsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ItemDto>> Handle(GetAllItemsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<Item>()
            .Include(i => i.ItemStatus)
            .Include(i => i.Brand)
            .Include(i => i.Category)
            .Include(i => i.ItemTags)
            .ThenInclude(it => it.Tag)
            .Include(i => i.Inventory)
            .AsQueryable();

        if (request.CategoryId.HasValue)
        {
            query = query.Where(i => i.CategoryId == request.CategoryId.Value);
        }

        if (request.BrandId.HasValue)
        {
            query = query.Where(i => i.BrandId == request.BrandId.Value);
        }

        if (request.ItemStatusId.HasValue)
        {
            query = query.Where(i => i.ItemStatusId == request.ItemStatusId.Value);
        }

        if (request.TagId.HasValue)
        {
            query = query.Where(i => i.ItemTags.Any(it => it.TagId == request.TagId.Value));
        }

        if (request.MinPrice.HasValue)
        {
            query = query.Where(i => i.Price >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            query = query.Where(i => i.Price <= request.MaxPrice.Value);
        }

        var items = await query.ToListAsync(cancellationToken);
        
        // Get primary images for all items
        var itemIds = items.Select(i => i.Id).ToList();
        var primaryImages = await _context.Set<EntityMediaLink>()
            .Include(eml => eml.MediaAsset)
            .Where(eml => eml.EntityType == "Item" && itemIds.Contains(eml.EntityId) && eml.IsPrimary)
            .ToDictionaryAsync(eml => eml.EntityId, eml => eml.MediaAsset.FileUrl, cancellationToken);

        return items.Select(item => {
            var dto = CreateItemCommandHandler.MapToDto(item);
            dto.PrimaryImageUrl = primaryImages.GetValueOrDefault(item.Id);
            return dto;
        }).ToList();
    }
}

