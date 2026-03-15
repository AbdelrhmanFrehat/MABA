using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Cms.Pages.Commands;
using Maba.Application.Features.Cms.DTOs;
using Maba.Domain.Cms;

namespace Maba.Application.Features.Cms.Pages.Handlers;

public class CreatePageCommandHandler : IRequestHandler<CreatePageCommand, PageDto>
{
    private readonly IApplicationDbContext _context;

    public CreatePageCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PageDto> Handle(CreatePageCommand request, CancellationToken cancellationToken)
    {
        // Check if key already exists
        var existingKey = await _context.Set<Page>()
            .AnyAsync(p => p.Key == request.Key, cancellationToken);

        if (existingKey)
        {
            throw new InvalidOperationException("Page with this key already exists");
        }

        // If this is set as home, unset other home pages
        if (request.IsHome)
        {
            var homePages = await _context.Set<Page>()
                .Where(p => p.IsHome)
                .ToListAsync(cancellationToken);

            foreach (var homePage in homePages)
            {
                homePage.IsHome = false;
            }
        }

        var page = new Page
        {
            Id = Guid.NewGuid(),
            Key = request.Key,
            Path = request.Path,
            TitleEn = request.TitleEn,
            TitleAr = request.TitleAr,
            MetaTitleEn = request.MetaTitleEn,
            MetaTitleAr = request.MetaTitleAr,
            MetaDescriptionEn = request.MetaDescriptionEn,
            MetaDescriptionAr = request.MetaDescriptionAr,
            IsHome = request.IsHome,
            IsActive = request.IsActive
        };

        _context.Set<Page>().Add(page);
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
            CreatedAt = page.CreatedAt,
            UpdatedAt = page.UpdatedAt
        };
    }
}

