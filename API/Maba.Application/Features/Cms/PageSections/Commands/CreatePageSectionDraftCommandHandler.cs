using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Cms.PageSections.Commands;
using Maba.Application.Features.Cms.DTOs;
using Maba.Domain.Cms;

namespace Maba.Application.Features.Cms.PageSections.Handlers;

public class CreatePageSectionDraftCommandHandler : IRequestHandler<CreatePageSectionDraftCommand, PageSectionDraftDto>
{
    private readonly IApplicationDbContext _context;

    public CreatePageSectionDraftCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PageSectionDraftDto> Handle(CreatePageSectionDraftCommand request, CancellationToken cancellationToken)
    {
        var page = await _context.Set<Page>()
            .FirstOrDefaultAsync(p => p.Id == request.PageId, cancellationToken);

        if (page == null)
        {
            throw new KeyNotFoundException("Page not found");
        }

        var sectionType = await _context.Set<PageSectionType>()
            .FirstOrDefaultAsync(pst => pst.Id == request.PageSectionTypeId, cancellationToken);

        if (sectionType == null)
        {
            throw new KeyNotFoundException("Page section type not found");
        }

        var layoutType = await _context.Set<LayoutType>()
            .FirstOrDefaultAsync(lt => lt.Id == request.LayoutTypeId, cancellationToken);

        if (layoutType == null)
        {
            throw new KeyNotFoundException("Layout type not found");
        }

        var draft = new PageSectionDraft
        {
            Id = Guid.NewGuid(),
            PageId = request.PageId,
            PageSectionTypeId = request.PageSectionTypeId,
            LayoutTypeId = request.LayoutTypeId,
            TitleEn = request.TitleEn,
            TitleAr = request.TitleAr,
            SubtitleEn = request.SubtitleEn,
            SubtitleAr = request.SubtitleAr,
            ConfigJson = request.ConfigJson,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive,
            CreatedByUserId = request.CreatedByUserId,
            UpdatedByUserId = request.CreatedByUserId
        };

        _context.Set<PageSectionDraft>().Add(draft);
        await _context.SaveChangesAsync(cancellationToken);

        return new PageSectionDraftDto
        {
            Id = draft.Id,
            PageId = draft.PageId,
            PageSectionTypeId = draft.PageSectionTypeId,
            PageSectionTypeKey = sectionType.Key,
            LayoutTypeId = draft.LayoutTypeId,
            LayoutTypeKey = layoutType.Key,
            TitleEn = draft.TitleEn,
            TitleAr = draft.TitleAr,
            SubtitleEn = draft.SubtitleEn,
            SubtitleAr = draft.SubtitleAr,
            ConfigJson = draft.ConfigJson,
            SortOrder = draft.SortOrder,
            IsActive = draft.IsActive,
            CreatedByUserId = draft.CreatedByUserId,
            UpdatedByUserId = draft.UpdatedByUserId,
            CreatedAt = draft.CreatedAt,
            UpdatedAt = draft.UpdatedAt
        };
    }
}

