using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Cnc.DTOs;
using Maba.Domain.Cnc;

namespace Maba.Application.Features.Cnc.Requests.Queries;

public class GetAllCncServiceRequestsQueryHandler : IRequestHandler<GetAllCncServiceRequestsQuery, CncServiceRequestsListResult>
{
    private const int DefaultTake = 25;
    private const int MaxTake = 500;

    private readonly IApplicationDbContext _context;

    public GetAllCncServiceRequestsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CncServiceRequestsListResult> Handle(GetAllCncServiceRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<CncServiceRequest>().AsQueryable();

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.ServiceMode))
        {
            var mode = request.ServiceMode.Trim();
            query = query.Where(x => x.ServiceMode == mode);
        }

        if (request.CreatedFromUtc.HasValue)
        {
            var from = request.CreatedFromUtc.Value.Date;
            query = query.Where(x => x.CreatedAt >= from);
        }

        if (request.CreatedToUtc.HasValue)
        {
            var toExclusive = request.CreatedToUtc.Value.Date.AddDays(1);
            query = query.Where(x => x.CreatedAt < toExclusive);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            if (Guid.TryParse(term, out var id))
            {
                query = query.Where(x => x.Id == id);
            }
            else
            {
                var termLower = term.ToLowerInvariant();
                query = query.Where(x =>
                    x.ReferenceNumber.ToLower().Contains(termLower) ||
                    x.CustomerName.ToLower().Contains(termLower) ||
                    x.CustomerEmail.ToLower().Contains(termLower) ||
                    (x.FileName != null && x.FileName.ToLower().Contains(termLower)));
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var skip = Math.Max(0, request.Skip ?? 0);
        var take = request.Take ?? DefaultTake;
        if (take < 1)
        {
            take = DefaultTake;
        }

        if (take > MaxTake)
        {
            take = MaxTake;
        }

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Take(take)
            .Select(x => new CncServiceRequestDto
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
            })
            .ToListAsync(cancellationToken);

        return new CncServiceRequestsListResult
        {
            Items = items,
            TotalCount = totalCount
        };
    }
}
