using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Laser.DTOs;
using Maba.Domain.Laser;

namespace Maba.Application.Features.Laser.Requests.Queries;

public class GetAllLaserServiceRequestsQueryHandler : IRequestHandler<GetAllLaserServiceRequestsQuery, List<LaserServiceRequestDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllLaserServiceRequestsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<LaserServiceRequestDto>> Handle(GetAllLaserServiceRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<LaserServiceRequest>()
            .Include(r => r.Material)
            .AsQueryable();

        if (request.Status.HasValue)
        {
            query = query.Where(r => r.Status == request.Status.Value);
        }

        query = query.OrderByDescending(r => r.CreatedAt);

        if (request.Offset.HasValue)
        {
            query = query.Skip(request.Offset.Value);
        }

        if (request.Limit.HasValue)
        {
            query = query.Take(request.Limit.Value);
        }

        var requests = await query.ToListAsync(cancellationToken);

        return requests.Select(r => new LaserServiceRequestDto
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
        }).ToList();
    }
}
