using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Cnc.DTOs;
using Maba.Domain.Cnc;

namespace Maba.Application.Features.Cnc.Requests.Queries;

public class GetAllCncServiceRequestsQueryHandler : IRequestHandler<GetAllCncServiceRequestsQuery, List<CncServiceRequestDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllCncServiceRequestsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CncServiceRequestDto>> Handle(GetAllCncServiceRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<CncServiceRequest>()
            .Include(x => x.Material)
            .AsQueryable();

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.ServiceMode))
        {
            query = query.Where(x => x.ServiceMode == request.ServiceMode);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(x => 
                x.ReferenceNumber.ToLower().Contains(term) ||
                x.CustomerName.ToLower().Contains(term) ||
                x.CustomerEmail.ToLower().Contains(term));
        }

        query = query.OrderByDescending(x => x.CreatedAt);

        if (request.Skip.HasValue)
        {
            query = query.Skip(request.Skip.Value);
        }

        if (request.Take.HasValue)
        {
            query = query.Take(request.Take.Value);
        }

        return await query.Select(x => new CncServiceRequestDto
        {
            Id = x.Id,
            ReferenceNumber = x.ReferenceNumber,
            ServiceMode = x.ServiceMode,
            MaterialId = x.MaterialId,
            MaterialNameEn = x.Material != null ? x.Material.NameEn : null,
            MaterialNameAr = x.Material != null ? x.Material.NameAr : null,
            PcbMaterial = x.PcbMaterial,
            PcbThickness = x.PcbThickness,
            PcbSide = x.PcbSide,
            PcbOperation = x.PcbOperation,
            OperationType = x.OperationType,
            WidthMm = x.WidthMm,
            HeightMm = x.HeightMm,
            ThicknessMm = x.ThicknessMm,
            Quantity = x.Quantity,
            DepthMode = x.DepthMode,
            DepthMm = x.DepthMm,
            DesignSourceType = x.DesignSourceType,
            FilePath = x.FilePath,
            FileName = x.FileName,
            DesignNotes = x.DesignNotes,
            CustomerName = x.CustomerName,
            CustomerEmail = x.CustomerEmail,
            CustomerPhone = x.CustomerPhone,
            ProjectDescription = x.ProjectDescription,
            AdminNotes = x.AdminNotes,
            EstimatedPrice = x.EstimatedPrice,
            FinalPrice = x.FinalPrice,
            Status = x.Status,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
            ReviewedAt = x.ReviewedAt,
            CompletedAt = x.CompletedAt
        }).ToListAsync(cancellationToken);
    }
}
