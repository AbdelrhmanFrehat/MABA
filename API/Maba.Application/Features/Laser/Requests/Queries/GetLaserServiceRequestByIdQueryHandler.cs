using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Laser.DTOs;
using Maba.Domain.Laser;

namespace Maba.Application.Features.Laser.Requests.Queries;

public class GetLaserServiceRequestByIdQueryHandler : IRequestHandler<GetLaserServiceRequestByIdQuery, LaserServiceRequestDto?>
{
    private readonly IApplicationDbContext _context;

    public GetLaserServiceRequestByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LaserServiceRequestDto?> Handle(GetLaserServiceRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var r = await _context.Set<LaserServiceRequest>()
            .Include(req => req.Material)
            .FirstOrDefaultAsync(req => req.Id == request.Id, cancellationToken);

        if (r == null)
        {
            return null;
        }

        return new LaserServiceRequestDto
        {
            Id = r.Id,
            ReferenceNumber = r.ReferenceNumber,
            MaterialId = r.MaterialId,
            MaterialNameEn = r.Material?.NameEn ?? "",
            MaterialNameAr = r.Material?.NameAr ?? "",
            OperationMode = r.OperationMode,
            ImagePath = r.ImagePath,
            ImageFileName = r.ImageFileName,
            WidthCm = r.WidthCm,
            HeightCm = r.HeightCm,
            CustomerName = r.CustomerName,
            CustomerEmail = r.CustomerEmail,
            CustomerPhone = r.CustomerPhone,
            CustomerNotes = r.CustomerNotes,
            AdminNotes = r.AdminNotes,
            QuotedPrice = r.QuotedPrice,
            Status = r.Status,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt,
            ReviewedAt = r.ReviewedAt,
            CompletedAt = r.CompletedAt
        };
    }
}
