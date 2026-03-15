using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Cms.Pages.Commands;
using Maba.Application.Features.Cms.DTOs;
using Maba.Domain.Cms;

namespace Maba.Application.Features.Cms.Pages.Handlers;

public class PublishPageCommandHandler : IRequestHandler<PublishPageCommand, PageDto>
{
    private readonly IApplicationDbContext _context;

    public PublishPageCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PageDto> Handle(PublishPageCommand request, CancellationToken cancellationToken)
    {
        var page = await _context.Set<Page>()
            .Include(p => p.DraftSections)
            .ThenInclude(ds => ds.Items)
            .Include(p => p.PublishedByUser)
            .FirstOrDefaultAsync(p => p.Id == request.PageId, cancellationToken);

        if (page == null)
        {
            throw new KeyNotFoundException("Page not found.");
        }

        // Unpublish existing published sections
        var existingPublishedSections = await _context.Set<PageSectionPublished>()
            .Where(ps => ps.PageId == request.PageId)
            .ToListAsync(cancellationToken);

        foreach (var section in existingPublishedSections)
        {
            section.UnpublishedAt = DateTime.UtcNow;
            section.UnpublishedByUserId = request.PublishedByUserId;
        }

        // Create published sections from draft sections
        foreach (var draftSection in page.DraftSections.Where(ds => ds.IsActive))
        {
            var publishedSection = new PageSectionPublished
            {
                Id = Guid.NewGuid(),
                PageId = page.Id,
                PageSectionTypeId = draftSection.PageSectionTypeId,
                LayoutTypeId = draftSection.LayoutTypeId,
                TitleEn = draftSection.TitleEn,
                TitleAr = draftSection.TitleAr,
                SubtitleEn = draftSection.SubtitleEn,
                SubtitleAr = draftSection.SubtitleAr,
                ConfigJson = draftSection.ConfigJson,
                SortOrder = draftSection.SortOrder,
                IsActive = draftSection.IsActive,
                PublishedAt = DateTime.UtcNow,
                PublishedByUserId = request.PublishedByUserId,
                Version = page.Version
            };

            _context.Set<PageSectionPublished>().Add(publishedSection);

            // Copy items
            foreach (var draftItem in draftSection.Items)
            {
                var publishedItem = new PageSectionItemPublished
                {
                    Id = Guid.NewGuid(),
                    PageSectionPublishedId = publishedSection.Id,
                    LinkedEntityType = draftItem.LinkedEntityType,
                    LinkedEntityId = draftItem.LinkedEntityId,
                    ExtraConfigJson = draftItem.ExtraConfigJson,
                    SortOrder = draftItem.SortOrder
                };

                _context.Set<PageSectionItemPublished>().Add(publishedItem);
            }
        }

        // Update page
        page.IsPublished = true;
        page.PublishedAt = DateTime.UtcNow;
        page.PublishedByUserId = request.PublishedByUserId;
        page.UpdatedAt = DateTime.UtcNow;

        // Create version snapshot
        var pageVersion = new PageVersion
        {
            Id = Guid.NewGuid(),
            PageId = page.Id,
            VersionNumber = page.Version,
            TitleEn = page.TitleEn,
            TitleAr = page.TitleAr,
            MetaTitleEn = page.MetaTitleEn,
            MetaTitleAr = page.MetaTitleAr,
            MetaDescriptionEn = page.MetaDescriptionEn,
            MetaDescriptionAr = page.MetaDescriptionAr,
            CreatedByUserId = request.PublishedByUserId
        };

        _context.Set<PageVersion>().Add(pageVersion);

        await _context.SaveChangesAsync(cancellationToken);

        return new PageDto
        {
            Id = page.Id,
            Key = page.Key,
            Path = page.Path,
            TitleEn = page.TitleEn,
            TitleAr = page.TitleAr,
            MetaTitleEn = page.MetaTitleEn,
            MetaTitleAr = page.MetaTitleAr,
            MetaDescriptionEn = page.MetaDescriptionEn,
            MetaDescriptionAr = page.MetaDescriptionAr,
            IsHome = page.IsHome,
            IsActive = page.IsActive,
            TemplateKey = page.TemplateKey,
            IsPublished = page.IsPublished,
            Version = page.Version,
            PublishedAt = page.PublishedAt,
            PublishedByUserId = page.PublishedByUserId,
            PublishedByUserName = page.PublishedByUser?.FullName,
            CreatedAt = page.CreatedAt,
            UpdatedAt = page.UpdatedAt
        };
    }
}

