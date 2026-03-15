using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Cms.Pages.Queries;
using Maba.Application.Features.Cms.DTOs;
using Maba.Domain.Cms;

namespace Maba.Application.Features.Cms.Pages.Handlers;

public class GetPageDetailQueryHandler : IRequestHandler<GetPageDetailQuery, PageDetailDto>
{
    private readonly IApplicationDbContext _context;

    public GetPageDetailQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PageDetailDto> Handle(GetPageDetailQuery request, CancellationToken cancellationToken)
    {
        var page = await _context.Set<Page>()
            .Include(p => p.PublishedByUser)
            .Include(p => p.DraftSections)
            .ThenInclude(ds => ds.PageSectionType)
            .Include(p => p.DraftSections)
            .ThenInclude(ds => ds.LayoutType)
            .Include(p => p.PublishedSections)
            .ThenInclude(ps => ps.PageSectionType)
            .Include(p => p.PublishedSections)
            .ThenInclude(ps => ps.LayoutType)
            .FirstOrDefaultAsync(p => p.Id == request.PageId, cancellationToken);

        if (page == null)
        {
            throw new KeyNotFoundException("Page not found.");
        }

        return new PageDetailDto
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
            DraftSections = page.DraftSections
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
            PublishedSections = page.PublishedSections
                .Where(ps => ps.UnpublishedAt == null)
                .OrderBy(ps => ps.SortOrder)
                .Select(ps => new PageSectionPublishedDto
                {
                    Id = ps.Id,
                    PageId = ps.PageId,
                    PageSectionTypeId = ps.PageSectionTypeId,
                    PageSectionTypeKey = ps.PageSectionType.Key,
                    LayoutTypeId = ps.LayoutTypeId,
                    LayoutTypeKey = ps.LayoutType.Key,
                    TitleEn = ps.TitleEn,
                    TitleAr = ps.TitleAr,
                    SubtitleEn = ps.SubtitleEn,
                    SubtitleAr = ps.SubtitleAr,
                    ConfigJson = ps.ConfigJson,
                    SortOrder = ps.SortOrder,
                    IsActive = ps.IsActive,
                    PublishedAt = ps.PublishedAt,
                    PublishedByUserId = ps.PublishedByUserId,
                    Version = ps.Version,
                    UnpublishedAt = ps.UnpublishedAt,
                    UnpublishedByUserId = ps.UnpublishedByUserId,
                    CreatedAt = ps.CreatedAt,
                    UpdatedAt = ps.UpdatedAt
                })
                .ToList(),
            CreatedAt = page.CreatedAt,
            UpdatedAt = page.UpdatedAt
        };
    }
}

