using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Printing.PrintQualityProfiles.DTOs;
using Maba.Domain.Printing;

namespace Maba.Application.Features.Printing.PrintQualityProfiles.Queries;

public class GetPrintQualityProfileByIdQueryHandler : IRequestHandler<GetPrintQualityProfileByIdQuery, PrintQualityProfileDto?>
{
    private readonly IApplicationDbContext _context;

    public GetPrintQualityProfileByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PrintQualityProfileDto?> Handle(GetPrintQualityProfileByIdQuery request, CancellationToken cancellationToken)
    {
        var profile = await _context.Set<PrintQualityProfile>()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (profile == null)
        {
            return null;
        }

        return new PrintQualityProfileDto
        {
            Id = profile.Id,
            NameEn = profile.NameEn,
            NameAr = profile.NameAr,
            DescriptionEn = profile.DescriptionEn,
            DescriptionAr = profile.DescriptionAr,
            LayerHeightMm = profile.LayerHeightMm,
            SpeedCategory = profile.SpeedCategory,
            PriceMultiplier = profile.PriceMultiplier,
            IsDefault = profile.IsDefault,
            IsActive = profile.IsActive,
            SortOrder = profile.SortOrder,
            CreatedAt = profile.CreatedAt,
            UpdatedAt = profile.UpdatedAt
        };
    }
}
