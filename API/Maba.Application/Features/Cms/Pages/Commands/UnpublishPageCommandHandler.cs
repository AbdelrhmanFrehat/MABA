using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Cms.Pages.Commands;
using Maba.Application.Features.Cms.DTOs;
using Maba.Domain.Cms;

namespace Maba.Application.Features.Cms.Pages.Handlers;

public class UnpublishPageCommandHandler : IRequestHandler<UnpublishPageCommand, PageDto>
{
    private readonly IApplicationDbContext _context;

    public UnpublishPageCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PageDto> Handle(UnpublishPageCommand request, CancellationToken cancellationToken)
    {
        var page = await _context.Set<Page>()
            .Include(p => p.PublishedByUser)
            .FirstOrDefaultAsync(p => p.Id == request.PageId, cancellationToken);

        if (page == null)
        {
            throw new KeyNotFoundException("Page not found.");
        }

        if (!page.IsPublished)
        {
            throw new InvalidOperationException("Page is not published.");
        }

        // Mark all published sections as unpublished
        var publishedSections = await _context.Set<PageSectionPublished>()
            .Where(ps => ps.PageId == request.PageId)
            .ToListAsync(cancellationToken);

        foreach (var section in publishedSections)
        {
            section.UnpublishedAt = DateTime.UtcNow;
            section.UnpublishedByUserId = request.UnpublishedByUserId;
        }

        // Update page
        page.IsPublished = false;
        page.UpdatedAt = DateTime.UtcNow;

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

