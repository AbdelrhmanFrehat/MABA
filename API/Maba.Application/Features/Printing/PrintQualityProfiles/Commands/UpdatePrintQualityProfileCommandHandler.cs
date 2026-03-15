using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Printing.PrintQualityProfiles.DTOs;
using Maba.Domain.Printing;

namespace Maba.Application.Features.Printing.PrintQualityProfiles.Commands;

public class UpdatePrintQualityProfileCommandHandler : IRequestHandler<UpdatePrintQualityProfileCommand, PrintQualityProfileDto?>
{
    private readonly IApplicationDbContext _context;

    public UpdatePrintQualityProfileCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PrintQualityProfileDto?> Handle(UpdatePrintQualityProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _context.Set<PrintQualityProfile>()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (profile == null)
        {
            return null;
        }

        profile.NameEn = request.NameEn;
        profile.NameAr = request.NameAr;
        profile.DescriptionEn = request.DescriptionEn;
        profile.DescriptionAr = request.DescriptionAr;
        profile.LayerHeightMm = request.LayerHeightMm;
        profile.SpeedCategory = request.SpeedCategory;
        profile.PriceMultiplier = request.PriceMultiplier;
        profile.IsDefault = request.IsDefault;
        profile.IsActive = request.IsActive;
        profile.SortOrder = request.SortOrder;
        profile.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

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
