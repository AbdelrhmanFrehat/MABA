using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Machines.DTOs;
using Maba.Application.Features.Machines.Queries;
using Maba.Domain.Machines;

namespace Maba.Application.Features.Machines.Handlers;

public class GetAllMachinesQueryHandler : IRequestHandler<GetAllMachinesQuery, List<MachineDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllMachinesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MachineDto>> Handle(GetAllMachinesQuery request, CancellationToken cancellationToken)
    {
        var machines = await _context.Set<Machine>()
            .Include(m => m.Parts)
            .ToListAsync(cancellationToken);

        return machines.Select(m => new MachineDto
        {
            Id = m.Id,
            NameEn = m.NameEn,
            NameAr = m.NameAr,
            Manufacturer = m.Manufacturer,
            Model = m.Model,
            YearFrom = m.YearFrom,
            YearTo = m.YearTo,
            Parts = m.Parts.Select(p => new MachinePartDto
            {
                Id = p.Id,
                MachineId = p.MachineId,
                PartNameEn = p.PartNameEn,
                PartNameAr = p.PartNameAr,
                PartCode = p.PartCode,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }).ToList(),
            CreatedAt = m.CreatedAt,
            UpdatedAt = m.UpdatedAt
        }).ToList();
    }
}

