using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Catalog.Tags.Commands;
using Maba.Application.Features.Catalog.Tags.DTOs;
using Maba.Domain.Catalog;

namespace Maba.Application.Features.Catalog.Tags.Handlers;

public class UpdateTagCommandHandler : IRequestHandler<UpdateTagCommand, TagDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateTagCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TagDto> Handle(UpdateTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await _context.Set<Tag>()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (tag == null)
        {
            throw new KeyNotFoundException("Tag not found");
        }

        tag.NameEn = request.NameEn;
        tag.NameAr = request.NameAr;
        tag.Slug = request.Slug;
        tag.IsActive = request.IsActive;
        tag.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

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

