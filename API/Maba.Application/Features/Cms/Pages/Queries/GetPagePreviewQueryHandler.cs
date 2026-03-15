using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Cms.Pages.Queries;
using Maba.Application.Features.Cms.DTOs;
using Maba.Domain.Cms;

namespace Maba.Application.Features.Cms.Pages.Handlers;

public class GetPagePreviewQueryHandler : IRequestHandler<GetPagePreviewQuery, PagePreviewDto>
{
    private readonly IApplicationDbContext _context;

    public GetPagePreviewQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagePreviewDto> Handle(GetPagePreviewQuery request, CancellationToken cancellationToken)
    {
        var page = await _context.Set<Page>()
            .Include(p => p.PublishedByUser)
            .Include(p => p.DraftSections)
            .ThenInclude(ds => ds.PageSectionType)
            .Include(p => p.DraftSections)
            .ThenInclude(ds => ds.LayoutType)
            .FirstOrDefaultAsync(p => p.Id == request.PageId, cancellationToken);

        if (page == null)
        {
            throw new KeyNotFoundException("Page not found.");
        }

        return new PagePreviewDto
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
            Sections = page.DraftSections
                .Where(ds => ds.IsActive)
                .OrderBy(ds => ds.SortOrder)
                .Select(ds => new PageSectionDraftDto
                {
                    Id = ds.Id,
                    PageId = ds.PageId,
                    PageSectionTypeId = ds.PageSectionTypeId,
                    PageSectionTypeKey = ds.PageSectionType.Key,
                    LayoutTypeId = ds.LayoutTypeId,
                    LayoutTypeKey = ds.LayoutType.Key,
                    TitleEn = ds.TitleEn,
                    TitleAr = ds.TitleAr,
                    SubtitleEn = ds.SubtitleEn,
                    SubtitleAr = ds.SubtitleAr,
                    ConfigJson = ds.ConfigJson,
                    SortOrder = ds.SortOrder,
                    IsActive = ds.IsActive,
                    CreatedByUserId = ds.CreatedByUserId,
                    UpdatedByUserId = ds.UpdatedByUserId,
                    PreviewUrl = ds.PreviewUrl,
                    CreatedAt = ds.CreatedAt,
                    UpdatedAt = ds.UpdatedAt
                })
                .ToList(),
            CreatedAt = page.CreatedAt,
            UpdatedAt = page.UpdatedAt
        };
    }
}

