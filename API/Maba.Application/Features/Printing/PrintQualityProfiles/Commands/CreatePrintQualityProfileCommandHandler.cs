using MediatR;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Printing.PrintQualityProfiles.DTOs;
using Maba.Domain.Printing;

namespace Maba.Application.Features.Printing.PrintQualityProfiles.Commands;

public class CreatePrintQualityProfileCommandHandler : IRequestHandler<CreatePrintQualityProfileCommand, PrintQualityProfileDto>
{
    private readonly IApplicationDbContext _context;

    public CreatePrintQualityProfileCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PrintQualityProfileDto> Handle(CreatePrintQualityProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = new PrintQualityProfile
        {
            Id = Guid.NewGuid(),
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            DescriptionEn = request.DescriptionEn,
            DescriptionAr = request.DescriptionAr,
            LayerHeightMm = request.LayerHeightMm,
            SpeedCategory = request.SpeedCategory,
            PriceMultiplier = request.PriceMultiplier,
            IsDefault = request.IsDefault,
            IsActive = request.IsActive,
            SortOrder = request.SortOrder,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<PrintQualityProfile>().Add(profile);
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
