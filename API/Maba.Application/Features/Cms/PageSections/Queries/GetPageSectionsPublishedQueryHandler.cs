using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Cms.DTOs;
using Maba.Application.Features.Cms.PageSections.Queries;
using Maba.Domain.Cms;

namespace Maba.Application.Features.Cms.PageSections.Handlers;

public class GetPageSectionsPublishedQueryHandler : IRequestHandler<GetPageSectionsPublishedQuery, List<PageSectionPublishedDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPageSectionsPublishedQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PageSectionPublishedDto>> Handle(GetPageSectionsPublishedQuery request, CancellationToken cancellationToken)
    {
        var published = await _context.Set<PageSectionPublished>()
            .Include(p => p.PageSectionType)
            .Include(p => p.LayoutType)
            .Where(p => p.PageId == request.PageId && p.IsActive)
            .OrderBy(p => p.SortOrder)
            .ToListAsync(cancellationToken);

        return published.Select(p => new PageSectionPublishedDto
        {
            Id = p.Id,
            PageId = p.PageId,
            PageSectionTypeId = p.PageSectionTypeId,
            PageSectionTypeKey = p.PageSectionType.Key,
            LayoutTypeId = p.LayoutTypeId,
            LayoutTypeKey = p.LayoutType.Key,
            TitleEn = p.TitleEn,
            TitleAr = p.TitleAr,
            SubtitleEn = p.SubtitleEn,
            SubtitleAr = p.SubtitleAr,
            ConfigJson = p.ConfigJson,
            SortOrder = p.SortOrder,
            IsActive = p.IsActive,
            PublishedAt = p.PublishedAt,
            PublishedByUserId = p.PublishedByUserId,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        }).ToList();
    }
}

