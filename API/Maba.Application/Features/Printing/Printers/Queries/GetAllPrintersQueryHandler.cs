using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Printing.DTOs;
using Maba.Application.Features.Printing.Printers.Queries;
using Maba.Domain.Printing;

namespace Maba.Application.Features.Printing.Printers.Handlers;

public class GetAllPrintersQueryHandler : IRequestHandler<GetAllPrintersQuery, List<PrinterDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllPrintersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PrinterDto>> Handle(GetAllPrintersQuery request, CancellationToken cancellationToken)
    {
        var printers = await _context.Set<Printer>()
            .Include(p => p.PrintingTechnology)
            .ToListAsync(cancellationToken);

        return printers.Select(p => new PrinterDto
        {
            Id = p.Id,
            NameEn = p.NameEn,
            NameAr = p.NameAr,
            Vendor = p.Vendor,
            BuildVolumeX = p.BuildVolumeX,
            BuildVolumeY = p.BuildVolumeY,
            BuildVolumeZ = p.BuildVolumeZ,
            PrintingTechnologyId = p.PrintingTechnologyId,
            PrintingTechnologyKey = p.PrintingTechnology.Key,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        }).ToList();
    }
}

