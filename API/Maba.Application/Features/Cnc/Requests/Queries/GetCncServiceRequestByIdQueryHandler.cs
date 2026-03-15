using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Cnc.DTOs;
using Maba.Domain.Cnc;

namespace Maba.Application.Features.Cnc.Requests.Queries;

public class GetCncServiceRequestByIdQueryHandler : IRequestHandler<GetCncServiceRequestByIdQuery, CncServiceRequestDto?>
{
    private readonly IApplicationDbContext _context;

    public GetCncServiceRequestByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CncServiceRequestDto?> Handle(GetCncServiceRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _context.Set<CncServiceRequest>()
            .Include(x => x.Material)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (serviceRequest == null)
        {
            return null;
        }

        return new CncServiceRequestDto
        {
            Id = serviceRequest.Id,
            ReferenceNumber = serviceRequest.ReferenceNumber,
            ServiceMode = serviceRequest.ServiceMode,
            MaterialId = serviceRequest.MaterialId,
            MaterialNameEn = serviceRequest.Material?.NameEn,
            MaterialNameAr = serviceRequest.Material?.NameAr,
            PcbMaterial = serviceRequest.PcbMaterial,
            PcbThickness = serviceRequest.PcbThickness,
            PcbSide = serviceRequest.PcbSide,
            PcbOperation = serviceRequest.PcbOperation,
            OperationType = serviceRequest.OperationType,
            WidthMm = serviceRequest.WidthMm,
            HeightMm = serviceRequest.HeightMm,
            ThicknessMm = serviceRequest.ThicknessMm,
            Quantity = serviceRequest.Quantity,
            DepthMode = serviceRequest.DepthMode,
            DepthMm = serviceRequest.DepthMm,
            DesignSourceType = serviceRequest.DesignSourceType,
            FilePath = serviceRequest.FilePath,
            FileName = serviceRequest.FileName,
            DesignNotes = serviceRequest.DesignNotes,
            CustomerName = serviceRequest.CustomerName,
            CustomerEmail = serviceRequest.CustomerEmail,
            CustomerPhone = serviceRequest.CustomerPhone,
            ProjectDescription = serviceRequest.ProjectDescription,
            AdminNotes = serviceRequest.AdminNotes,
            EstimatedPrice = serviceRequest.EstimatedPrice,
            FinalPrice = serviceRequest.FinalPrice,
            Status = serviceRequest.Status,
            CreatedAt = serviceRequest.CreatedAt,
            UpdatedAt = serviceRequest.UpdatedAt,
            ReviewedAt = serviceRequest.ReviewedAt,
            CompletedAt = serviceRequest.CompletedAt
        };
    }
}
