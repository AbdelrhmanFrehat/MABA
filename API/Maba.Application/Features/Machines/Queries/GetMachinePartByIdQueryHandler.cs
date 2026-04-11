using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Machines.Queries;
using Maba.Application.Features.Machines.DTOs;
using Maba.Domain.Machines;

namespace Maba.Application.Features.Machines.Handlers;

public class GetMachinePartByIdQueryHandler : IRequestHandler<GetMachinePartByIdQuery, MachinePartDto>
{
    private readonly IApplicationDbContext _context;

    public GetMachinePartByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MachinePartDto> Handle(GetMachinePartByIdQuery request, CancellationToken cancellationToken)
    {
        var part = await _context.Set<MachinePart>()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Machine part {request.Id} not found.");

        return new MachinePartDto
        {
            Id = part.Id,
            MachineId = part.MachineId,
            PartNameEn = part.PartNameEn,
            PartNameAr = part.PartNameAr,
            PartCode = part.PartCode,
            Price = part.Price,
            InventoryId = part.InventoryId,
            ImageId = part.ImageId,
            CreatedAt = part.CreatedAt,
            UpdatedAt = part.UpdatedAt
        };
    }
}
