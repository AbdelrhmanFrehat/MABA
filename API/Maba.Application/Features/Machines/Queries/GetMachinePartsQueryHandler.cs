using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Machines.Queries;
using Maba.Application.Features.Machines.DTOs;
using Maba.Domain.Machines;

namespace Maba.Application.Features.Machines.Handlers;

public class GetMachinePartsQueryHandler : IRequestHandler<GetMachinePartsQuery, List<MachinePartDto>>
{
    private readonly IApplicationDbContext _context;

    public GetMachinePartsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MachinePartDto>> Handle(GetMachinePartsQuery request, CancellationToken cancellationToken)
    {
        var machine = await _context.Set<Machine>()
            .FirstOrDefaultAsync(m => m.Id == request.MachineId, cancellationToken);

        if (machine == null)
        {
            throw new KeyNotFoundException("Machine not found.");
        }

        var parts = await _context.Set<MachinePart>()
            .Where(p => p.MachineId == request.MachineId)
            .OrderBy(p => p.PartNameEn)
            .ToListAsync(cancellationToken);

        return parts.Select(p => new MachinePartDto
        {
            Id = p.Id,
            MachineId = p.MachineId,
            PartNameEn = p.PartNameEn,
            PartNameAr = p.PartNameAr,
            PartCode = p.PartCode,
            Price = p.Price,
            InventoryId = p.InventoryId,
            ImageId = p.ImageId,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        }).ToList();
    }
}

