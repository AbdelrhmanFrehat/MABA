using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Printing.PrintQualityProfiles.DTOs;
using Maba.Domain.Printing;

namespace Maba.Application.Features.Printing.PrintQualityProfiles.Queries;

public class GetAllPrintQualityProfilesQueryHandler : IRequestHandler<GetAllPrintQualityProfilesQuery, List<PrintQualityProfileDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllPrintQualityProfilesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PrintQualityProfileDto>> Handle(GetAllPrintQualityProfilesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<PrintQualityProfile>().AsQueryable();

        if (request.ActiveOnly)
        {
            query = query.Where(p => p.IsActive);
        }

        var profiles = await query.OrderBy(p => p.SortOrder).ToListAsync(cancellationToken);

        return profiles.Select(p => new PrintQualityProfileDto
        {
            Id = p.Id,
            NameEn = p.NameEn,
            NameAr = p.NameAr,
            DescriptionEn = p.DescriptionEn,
            DescriptionAr = p.DescriptionAr,
            LayerHeightMm = p.LayerHeightMm,
            SpeedCategory = p.SpeedCategory,
            PriceMultiplier = p.PriceMultiplier,
            IsDefault = p.IsDefault,
            IsActive = p.IsActive,
            SortOrder = p.SortOrder,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        }).ToList();
    }
}
