using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Cms.Pages.Queries;
using Maba.Application.Features.Cms.DTOs;
using Maba.Domain.Cms;

namespace Maba.Application.Features.Cms.Pages.Handlers;

public class GetPageByKeyQueryHandler : IRequestHandler<GetPageByKeyQuery, PageDto>
{
    private readonly IApplicationDbContext _context;

    public GetPageByKeyQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PageDto> Handle(GetPageByKeyQuery request, CancellationToken cancellationToken)
    {
        var page = await _context.Set<Page>()
            .Include(p => p.PublishedByUser)
            .FirstOrDefaultAsync(p => p.Key == request.Key && p.IsPublished && p.IsActive, cancellationToken);

        if (page == null)
        {
            throw new KeyNotFoundException($"Published page with key '{request.Key}' not found.");
        }

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

