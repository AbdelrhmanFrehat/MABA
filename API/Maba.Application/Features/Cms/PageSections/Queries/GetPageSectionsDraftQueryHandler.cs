using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Cms.DTOs;
using Maba.Application.Features.Cms.PageSections.Queries;
using Maba.Domain.Cms;

namespace Maba.Application.Features.Cms.PageSections.Handlers;

public class GetPageSectionsDraftQueryHandler : IRequestHandler<GetPageSectionsDraftQuery, List<PageSectionDraftDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPageSectionsDraftQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PageSectionDraftDto>> Handle(GetPageSectionsDraftQuery request, CancellationToken cancellationToken)
    {
        var drafts = await _context.Set<PageSectionDraft>()
            .Include(d => d.PageSectionType)
            .Include(d => d.LayoutType)
            .Where(d => d.PageId == request.PageId)
            .OrderBy(d => d.SortOrder)
            .ToListAsync(cancellationToken);

        return drafts.Select(d => new PageSectionDraftDto
        {
            Id = d.Id,
            PageId = d.PageId,
            PageSectionTypeId = d.PageSectionTypeId,
            PageSectionTypeKey = d.PageSectionType.Key,
            LayoutTypeId = d.LayoutTypeId,
            LayoutTypeKey = d.LayoutType.Key,
            TitleEn = d.TitleEn,
            TitleAr = d.TitleAr,
            SubtitleEn = d.SubtitleEn,
            SubtitleAr = d.SubtitleAr,
            ConfigJson = d.ConfigJson,
            SortOrder = d.SortOrder,
            IsActive = d.IsActive,
            CreatedByUserId = d.CreatedByUserId,
            UpdatedByUserId = d.UpdatedByUserId,
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt
        }).ToList();
    }
}

