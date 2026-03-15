using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Tags.DTOs;
using Maba.Application.Features.Catalog.Tags.Queries;
using Maba.Domain.Catalog;

namespace Maba.Application.Features.Catalog.Tags.Handlers;

public class GetTagByIdQueryHandler : IRequestHandler<GetTagByIdQuery, TagDto>
{
    private readonly IApplicationDbContext _context;

    public GetTagByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TagDto> Handle(GetTagByIdQuery request, CancellationToken cancellationToken)
    {
        var tag = await _context.Set<Tag>()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (tag == null)
        {
            throw new KeyNotFoundException("Tag not found");
        }

        return new TagDto
        {
            Id = tag.Id,
            NameEn = tag.NameEn,
            NameAr = tag.NameAr,
            Slug = tag.Slug,
            IsActive = tag.IsActive,
            CreatedAt = tag.CreatedAt,
            UpdatedAt = tag.UpdatedAt
        };
    }
}

