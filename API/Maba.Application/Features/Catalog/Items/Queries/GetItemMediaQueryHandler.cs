using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Items.Commands;
using Maba.Domain.Media;

namespace Maba.Application.Features.Catalog.Items.Queries;

public class GetItemMediaQueryHandler : IRequestHandler<GetItemMediaQuery, List<ItemMediaDto>>
{
    private readonly IApplicationDbContext _context;

    public GetItemMediaQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ItemMediaDto>> Handle(GetItemMediaQuery request, CancellationToken cancellationToken)
    {
        var links = await _context.Set<EntityMediaLink>()
            .Include(eml => eml.MediaAsset)
            .ThenInclude(m => m.MediaType)
            .Where(eml => eml.EntityType == "Item" && eml.EntityId == request.ItemId)
            .OrderBy(eml => eml.SortOrder)
            .ToListAsync(cancellationToken);

        return links.Select(link => new ItemMediaDto
        {
            Id = link.Id,
            ItemId = request.ItemId,
            MediaAssetId = link.MediaAssetId,
            FileUrl = link.MediaAsset.FileUrl,
            TitleEn = link.MediaAsset.TitleEn,
            TitleAr = link.MediaAsset.TitleAr,
            AltTextEn = link.MediaAsset.AltTextEn,
            AltTextAr = link.MediaAsset.AltTextAr,
            MediaType = link.MediaAsset.MediaType?.Key,
            IsPrimary = link.IsPrimary,
            SortOrder = link.SortOrder
        }).ToList();
    }
}
