using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Cms.DTOs;
using Maba.Application.Features.Cms.Pages.Queries;
using Maba.Domain.Cms;

namespace Maba.Application.Features.Cms.Pages.Handlers;

public class GetAllPagesQueryHandler : IRequestHandler<GetAllPagesQuery, List<PageDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllPagesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PageDto>> Handle(GetAllPagesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<Page>().AsQueryable();

        if (request.IsActive.HasValue)
        {
            query = query.Where(p => p.IsActive == request.IsActive.Value);
        }

        var pages = await query.ToListAsync(cancellationToken);

        return pages.Select(p => new PageDto
        {
            Id = p.Id,
            Key = p.Key,
            Path = p.Path,
            TitleEn = p.TitleEn,
            TitleAr = p.TitleAr,
            MetaTitleEn = p.MetaTitleEn,
            MetaTitleAr = p.MetaTitleAr,
            MetaDescriptionEn = p.MetaDescriptionEn,
            MetaDescriptionAr = p.MetaDescriptionAr,
            IsHome = p.IsHome,
            IsActive = p.IsActive,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        }).ToList();
    }
}

