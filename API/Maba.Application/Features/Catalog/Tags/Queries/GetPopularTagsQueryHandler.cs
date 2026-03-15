using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Tags.Queries;
using Maba.Application.Features.Catalog.Tags.DTOs;
using Maba.Domain.Catalog;

namespace Maba.Application.Features.Catalog.Tags.Handlers;

public class GetPopularTagsQueryHandler : IRequestHandler<GetPopularTagsQuery, List<TagDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPopularTagsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TagDto>> Handle(GetPopularTagsQuery request, CancellationToken cancellationToken)
    {
        var tags = await _context.Set<Tag>()
            .Where(t => t.IsActive)
            .OrderByDescending(t => t.UsageCount)
            .ThenBy(t => t.NameEn)
            .Take(request.Limit ?? 10)
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

