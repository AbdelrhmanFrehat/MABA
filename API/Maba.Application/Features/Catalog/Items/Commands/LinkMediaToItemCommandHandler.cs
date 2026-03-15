using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Media;
using Maba.Domain.Catalog;

namespace Maba.Application.Features.Catalog.Items.Commands;

public class LinkMediaToItemCommandHandler : IRequestHandler<LinkMediaToItemCommand, ItemMediaDto>
{
    private readonly IApplicationDbContext _context;

    public LinkMediaToItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ItemMediaDto> Handle(LinkMediaToItemCommand request, CancellationToken cancellationToken)
    {
        // Validate item exists
        var itemExists = await _context.Set<Item>()
            .AnyAsync(i => i.Id == request.ItemId, cancellationToken);

        if (!itemExists)
        {
            throw new KeyNotFoundException("Item not found");
        }

        // Validate media asset exists
        var mediaAsset = await _context.Set<MediaAsset>()
            .Include(m => m.MediaType)
            .FirstOrDefaultAsync(m => m.Id == request.MediaAssetId, cancellationToken);

        if (mediaAsset == null)
        {
            throw new KeyNotFoundException("Media asset not found");
        }

        // Get or create media usage type for Item images
        var usageType = await _context.Set<MediaUsageType>()
            .FirstOrDefaultAsync(u => u.Key == "ItemImage", cancellationToken);

        if (usageType == null)
        {
            usageType = new MediaUsageType
            {
                Id = Guid.NewGuid(),
                Key = "ItemImage",
                NameEn = "Item Image",
                NameAr = "صورة المنتج"
            };
            _context.Set<MediaUsageType>().Add(usageType);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // If setting as primary, unset other primary images
        if (request.IsPrimary)
        {
            var existingPrimary = await _context.Set<EntityMediaLink>()
                .Where(eml => eml.EntityType == "Item" && eml.EntityId == request.ItemId && eml.IsPrimary)
                .ToListAsync(cancellationToken);

            foreach (var existing in existingPrimary)
            {
                existing.IsPrimary = false;
            }
        }

        // Check if link already exists
        var existingLink = await _context.Set<EntityMediaLink>()
            .FirstOrDefaultAsync(eml => 
                eml.EntityType == "Item" && 
                eml.EntityId == request.ItemId && 
                eml.MediaAssetId == request.MediaAssetId, cancellationToken);

        if (existingLink != null)
        {
            // Update existing link
            existingLink.IsPrimary = request.IsPrimary;
            existingLink.SortOrder = request.SortOrder;
        }
        else
        {
            // Create new link
            existingLink = new EntityMediaLink
            {
                Id = Guid.NewGuid(),
                EntityType = "Item",
                EntityId = request.ItemId,
                MediaAssetId = request.MediaAssetId,
                MediaUsageTypeId = usageType.Id,
                IsPrimary = request.IsPrimary,
                SortOrder = request.SortOrder
            };
            _context.Set<EntityMediaLink>().Add(existingLink);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new ItemMediaDto
        {
            Id = existingLink.Id,
            ItemId = request.ItemId,
            MediaAssetId = request.MediaAssetId,
            FileUrl = mediaAsset.FileUrl,
            TitleEn = mediaAsset.TitleEn,
            TitleAr = mediaAsset.TitleAr,
            AltTextEn = mediaAsset.AltTextEn,
            AltTextAr = mediaAsset.AltTextAr,
            MediaType = mediaAsset.MediaType?.Key,
            IsPrimary = existingLink.IsPrimary,
            SortOrder = existingLink.SortOrder
        };
    }
}
