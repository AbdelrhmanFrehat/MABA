using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Tags.Queries;
using Maba.Application.Features.Catalog.Tags.DTOs;
using Maba.Domain.Catalog;

namespace Maba.Application.Features.Catalog.Tags.Handlers;

public class SearchTagsQueryHandler : IRequestHandler<SearchTagsQuery, List<TagDto>>
{
    private readonly IApplicationDbContext _context;

    public SearchTagsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TagDto>> Handle(SearchTagsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<Tag>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(t =>
                t.NameEn.ToLower().Contains(searchTerm) ||
                t.NameAr.ToLower().Contains(searchTerm) ||
                t.Slug.ToLower().Contains(searchTerm));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(t => t.IsActive == request.IsActive.Value);
        }

        var tags = await query
            .OrderBy(t => t.NameEn)
            .ToListAsync(cancellationToken);

        return tags.Select(t => new TagDto
        {
            Id = t.Id,
            NameEn = t.NameEn,
            NameAr = t.NameAr,
            Slug = t.Slug,
            IsActive = t.IsActive,
            Color = t.Color,
            Icon = t.Icon,
            UsageCount = t.UsageCount,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        }).ToList();
    }
}

