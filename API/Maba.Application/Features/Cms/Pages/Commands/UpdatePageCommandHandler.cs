using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Cms.Pages.Commands;
using Maba.Application.Features.Cms.DTOs;
using Maba.Domain.Cms;

namespace Maba.Application.Features.Cms.Pages.Handlers;

public class UpdatePageCommandHandler : IRequestHandler<UpdatePageCommand, PageDto>
{
    private readonly IApplicationDbContext _context;

    public UpdatePageCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PageDto> Handle(UpdatePageCommand request, CancellationToken cancellationToken)
    {
        var page = await _context.Set<Page>()
            .Include(p => p.PublishedByUser)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (page == null)
        {
            throw new KeyNotFoundException("Page not found.");
        }

        if (request.Key != null)
        {
            // Check if key is unique
            var existingPage = await _context.Set<Page>()
                .FirstOrDefaultAsync(p => p.Key == request.Key && p.Id != request.Id, cancellationToken);

            if (existingPage != null)
            {
                throw new InvalidOperationException($"Page with key '{request.Key}' already exists.");
            }

            page.Key = request.Key;
        }

        if (request.Path != null)
        {
            page.Path = request.Path;
        }

        if (request.TitleEn != null)
        {
            page.TitleEn = request.TitleEn;
        }

        if (request.TitleAr != null)
        {
            page.TitleAr = request.TitleAr;
        }

        if (request.MetaTitleEn != null)
        {
            page.MetaTitleEn = request.MetaTitleEn;
        }

        if (request.MetaTitleAr != null)
        {
            page.MetaTitleAr = request.MetaTitleAr;
        }

        if (request.MetaDescriptionEn != null)
        {
            page.MetaDescriptionEn = request.MetaDescriptionEn;
        }

        if (request.MetaDescriptionAr != null)
        {
            page.MetaDescriptionAr = request.MetaDescriptionAr;
        }

        if (request.IsHome.HasValue)
        {
            // If setting this page as home, unset other home pages
            if (request.IsHome.Value)
            {
                var otherHomePages = await _context.Set<Page>()
                    .Where(p => p.IsHome && p.Id != request.Id)
                    .ToListAsync(cancellationToken);

                foreach (var otherPage in otherHomePages)
                {
                    otherPage.IsHome = false;
                }
            }

            page.IsHome = request.IsHome.Value;
        }

        if (request.IsActive.HasValue)
        {
            page.IsActive = request.IsActive.Value;
        }

        if (request.TemplateKey != null)
        {
            page.TemplateKey = request.TemplateKey;
        }

        page.UpdatedAt = DateTime.UtcNow;
        page.Version++;

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

