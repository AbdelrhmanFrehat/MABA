using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Cms.PageSections.Commands;
using Maba.Application.Features.Cms.DTOs;
using Maba.Domain.Cms;

namespace Maba.Application.Features.Cms.PageSections.Handlers;

public class PublishPageSectionCommandHandler : IRequestHandler<PublishPageSectionCommand, PageSectionPublishedDto>
{
    private readonly IApplicationDbContext _context;

    public PublishPageSectionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PageSectionPublishedDto> Handle(PublishPageSectionCommand request, CancellationToken cancellationToken)
    {
        var draft = await _context.Set<PageSectionDraft>()
            .Include(d => d.PageSectionType)
            .Include(d => d.LayoutType)
            .Include(d => d.Items)
            .FirstOrDefaultAsync(d => d.Id == request.DraftId, cancellationToken);

        if (draft == null)
        {
            throw new KeyNotFoundException("Page section draft not found");
        }

        // Create published version
        var published = new PageSectionPublished
        {
            Id = Guid.NewGuid(),
            PageId = draft.PageId,
            PageSectionTypeId = draft.PageSectionTypeId,
            LayoutTypeId = draft.LayoutTypeId,
            TitleEn = draft.TitleEn,
            TitleAr = draft.TitleAr,
            SubtitleEn = draft.SubtitleEn,
            SubtitleAr = draft.SubtitleAr,
            ConfigJson = draft.ConfigJson,
            SortOrder = draft.SortOrder,
            IsActive = draft.IsActive,
            PublishedAt = DateTime.UtcNow,
            PublishedByUserId = request.PublishedByUserId
        };

        _context.Set<PageSectionPublished>().Add(published);
        await _context.SaveChangesAsync(cancellationToken);

        // Copy draft items to published items
        foreach (var draftItem in draft.Items)
        {
            var publishedItem = new PageSectionItemPublished
            {
                Id = Guid.NewGuid(),
                PageSectionPublishedId = published.Id,
                LinkedEntityType = draftItem.LinkedEntityType,
                LinkedEntityId = draftItem.LinkedEntityId,
                ExtraConfigJson = draftItem.ExtraConfigJson,
                SortOrder = draftItem.SortOrder
            };
            _context.Set<PageSectionItemPublished>().Add(publishedItem);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new PageSectionPublishedDto
        {
            Id = published.Id,
            PageId = published.PageId,
            PageSectionTypeId = published.PageSectionTypeId,
            PageSectionTypeKey = draft.PageSectionType.Key,
            LayoutTypeId = published.LayoutTypeId,
            LayoutTypeKey = draft.LayoutType.Key,
            TitleEn = published.TitleEn,
            TitleAr = published.TitleAr,
            SubtitleEn = published.SubtitleEn,
            SubtitleAr = published.SubtitleAr,
            ConfigJson = published.ConfigJson,
            SortOrder = published.SortOrder,
            IsActive = published.IsActive,
            PublishedAt = published.PublishedAt,
            PublishedByUserId = published.PublishedByUserId,
            CreatedAt = published.CreatedAt,
            UpdatedAt = published.UpdatedAt
        };
    }
}

