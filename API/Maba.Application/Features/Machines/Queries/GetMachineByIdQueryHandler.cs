using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Machines.DTOs;
using Maba.Application.Features.Machines.Queries;
using Maba.Domain.Machines;

namespace Maba.Application.Features.Machines.Handlers;

public class GetMachineByIdQueryHandler : IRequestHandler<GetMachineByIdQuery, MachineDto>
{
    private readonly IApplicationDbContext _context;

    public GetMachineByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MachineDto> Handle(GetMachineByIdQuery request, CancellationToken cancellationToken)
    {
        var machine = await _context.Set<Machine>()
            .Include(m => m.Parts)
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (machine == null)
        {
            throw new KeyNotFoundException("Machine not found");
        }

        return new MachineDto
        {
            Id = machine.Id,
            NameEn = machine.NameEn,
            NameAr = machine.NameAr,
            Manufacturer = machine.Manufacturer,
            Model = machine.Model,
            YearFrom = machine.YearFrom,
            YearTo = machine.YearTo,
            Parts = machine.Parts.Select(p => new MachinePartDto
            {
                Id = p.Id,
                MachineId = p.MachineId,
                PartNameEn = p.PartNameEn,
                PartNameAr = p.PartNameAr,
                PartCode = p.PartCode,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }).ToList(),
            CreatedAt = machine.CreatedAt,
            UpdatedAt = machine.UpdatedAt
        };
    }
}

