using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Machines.DTOs;
using Maba.Application.Features.Machines.Queries;
using Maba.Domain.Machines;

namespace Maba.Application.Features.Machines.Handlers;

public class GetItemMachineLinksQueryHandler : IRequestHandler<GetItemMachineLinksQuery, List<ItemMachineLinkDto>>
{
    private readonly IApplicationDbContext _context;

    public GetItemMachineLinksQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ItemMachineLinkDto>> Handle(GetItemMachineLinksQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<ItemMachineLink>()
            .Include(l => l.Machine)
            .Include(l => l.MachinePart)
            .AsQueryable();

        if (request.ItemId.HasValue)
        {
            query = query.Where(l => l.ItemId == request.ItemId.Value);
        }

        if (request.MachineId.HasValue)
        {
            query = query.Where(l => l.MachineId == request.MachineId.Value);
        }

        var links = await query.ToListAsync(cancellationToken);

        return links.Select(l => new ItemMachineLinkDto
        {
            Id = l.Id,
            ItemId = l.ItemId,
            MachineId = l.MachineId,
            MachinePartId = l.MachinePartId,
            MachineNameEn = l.Machine.NameEn,
            PartNameEn = l.MachinePart?.PartNameEn,
            CreatedAt = l.CreatedAt,
            UpdatedAt = l.UpdatedAt
        }).ToList();
    }
}

