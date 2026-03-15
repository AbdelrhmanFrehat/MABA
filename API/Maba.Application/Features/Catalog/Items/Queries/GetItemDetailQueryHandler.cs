using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Items.Queries;
using Maba.Application.Features.Catalog.Items.DTOs;
using Maba.Application.Features.Catalog.Items.Commands;
using Maba.Domain.Catalog;
using Maba.Domain.Media;

namespace Maba.Application.Features.Catalog.Items.Handlers;

public class GetItemDetailQueryHandler : IRequestHandler<GetItemDetailQuery, ItemDetailDto>
{
    private readonly IApplicationDbContext _context;

    public GetItemDetailQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ItemDetailDto> Handle(GetItemDetailQuery request, CancellationToken cancellationToken)
    {
        var item = await _context.Set<Item>()
            .Include(i => i.ItemStatus)
            .Include(i => i.Brand)
            .Include(i => i.Category)
            .Include(i => i.ItemTags)
            .ThenInclude(it => it.Tag)
            .Include(i => i.Inventory)
            .Include(i => i.ItemSections)
            .ThenInclude(s => s.Features)
            .Include(i => i.Reviews)
            .ThenInclude(r => r.User)
            .Include(i => i.Comments)
            .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(i => i.Id == request.ItemId, cancellationToken);

        if (item == null)
        {
            throw new KeyNotFoundException("Item not found.");
        }

        // Increment views count
        item.ViewsCount++;
        item.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        var mediaLinks = await _context.Set<EntityMediaLink>()
            .Include(eml => eml.MediaAsset)
            .ThenInclude(m => m.MediaType)
            .Where(eml => eml.EntityType == "Item" && eml.EntityId == item.Id)
            .OrderBy(eml => eml.SortOrder)
            .ToListAsync(cancellationToken);

        var mediaAssets = mediaLinks.Select(link => new ItemMediaDto
        {
            Id = link.Id,
            ItemId = item.Id,
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

        var primaryImageUrl = mediaLinks
            .Where(eml => eml.IsPrimary && eml.MediaAsset.MediaType?.Key == "Image")
            .Select(eml => eml.MediaAsset.FileUrl)
            .FirstOrDefault()
            ?? mediaLinks
                .Where(eml => eml.MediaAsset.MediaType?.Key == "Image")
                .OrderBy(eml => eml.SortOrder)
                .Select(eml => eml.MediaAsset.FileUrl)
                .FirstOrDefault();

        return new ItemDetailDto
        {
            Id = item.Id,
            NameEn = item.NameEn,
            NameAr = item.NameAr,
            Sku = item.Sku,
            GeneralDescriptionEn = item.GeneralDescriptionEn,
            GeneralDescriptionAr = item.GeneralDescriptionAr,
            ItemStatusId = item.ItemStatusId,
            ItemStatusKey = item.ItemStatus.Key,
            Price = item.Price,
            DiscountPrice = item.DiscountPrice,
            Currency = item.Currency,
            Weight = item.Weight,
            Dimensions = item.Dimensions,
            TaxRate = item.TaxRate,
            IsFeatured = item.IsFeatured,
            IsNew = item.IsNew,
            IsOnSale = item.IsOnSale,
            MinOrderQuantity = item.MinOrderQuantity,
            MaxOrderQuantity = item.MaxOrderQuantity,
            WarrantyPeriodMonths = item.WarrantyPeriodMonths,
            MetaTitleEn = item.MetaTitleEn,
            MetaTitleAr = item.MetaTitleAr,
            MetaDescriptionEn = item.MetaDescriptionEn,
            MetaDescriptionAr = item.MetaDescriptionAr,
            BrandId = item.BrandId,
            BrandNameEn = item.Brand?.NameEn,
            CategoryId = item.CategoryId,
            CategoryNameEn = item.Category?.NameEn,
            AverageRating = item.AverageRating,
            ReviewsCount = item.ReviewsCount,
            ViewsCount = item.ViewsCount,
            TagIds = item.ItemTags.Select(it => it.TagId).ToList(),
            Inventory = item.Inventory != null ? new InventoryDto
            {
                Id = item.Inventory.Id,
                ItemId = item.Inventory.ItemId,
                QuantityOnHand = item.Inventory.QuantityOnHand,
                QuantityReserved = item.Inventory.QuantityReserved,
                QuantityAvailable = item.Inventory.QuantityAvailable,
                QuantityOnOrder = item.Inventory.QuantityOnOrder,
                ReorderLevel = item.Inventory.ReorderLevel,
                CostPerUnit = item.Inventory.CostPerUnit,
                LastStockInAt = item.Inventory.LastStockInAt,
                LastStockOutAt = item.Inventory.LastStockOutAt
            } : null,
            PrimaryImageUrl = primaryImageUrl,
            MediaAssets = mediaAssets,
            Sections = item.ItemSections
                .OrderBy(s => s.SortOrder)
                .Select(s => new ItemSectionDto
                {
                    Id = s.Id,
                    ItemId = s.ItemId,
                    TitleEn = s.TitleEn,
                    TitleAr = s.TitleAr,
                    DescriptionEn = s.DescriptionEn,
                    DescriptionAr = s.DescriptionAr,
                    SortOrder = s.SortOrder,
                    Features = s.Features
                        .OrderBy(f => f.SortOrder)
                        .Select(f => new ItemSectionFeatureDto
                        {
                            Id = f.Id,
                            ItemSectionId = f.ItemSectionId,
                            TextEn = f.TextEn,
                            TextAr = f.TextAr,
                            SortOrder = f.SortOrder
                        })
                        .ToList()
                })
                .ToList(),
            Reviews = item.Reviews
                .Where(r => r.IsApproved)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    ItemId = r.ItemId,
                    UserId = r.UserId,
                    UserFullName = r.User.FullName,
                    Rating = r.Rating,
                    Title = r.Title,
                    Body = r.Body,
                    IsApproved = r.IsApproved,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                })
                .ToList(),
            Comments = item.Comments
                .Where(c => c.ParentCommentId == null && c.IsApproved)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    ItemId = c.ItemId,
                    UserId = c.UserId,
                    UserFullName = c.User.FullName,
                    Body = c.Body,
                    ParentCommentId = c.ParentCommentId,
                    IsApproved = c.IsApproved,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .ToList(),
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        };
    }
}

