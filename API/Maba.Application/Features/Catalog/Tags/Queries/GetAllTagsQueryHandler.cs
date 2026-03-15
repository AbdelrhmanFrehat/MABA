using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Tags.DTOs;
using Maba.Application.Features.Catalog.Tags.Queries;
using Maba.Domain.Catalog;

namespace Maba.Application.Features.Catalog.Tags.Handlers;

public class GetAllTagsQueryHandler : IRequestHandler<GetAllTagsQuery, List<TagDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllTagsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TagDto>> Handle(GetAllTagsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<Tag>().AsQueryable();

        if (request.IsActive.HasValue)
        {
            query = query.Where(t => t.IsActive == request.IsActive.Value);
        }

        var tags = await query.ToListAsync(cancellationToken);

        return tags.Select(t => new TagDto
        {
            Id = t.Id,
            NameEn = t.NameEn,
            NameAr = t.NameAr,
            Slug = t.Slug,
            IsActive = t.IsActive,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        }).ToList();
    }
}

