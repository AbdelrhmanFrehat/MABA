using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Machines.Queries;
using Maba.Application.Features.Machines.DTOs;
using Maba.Domain.Machines;

namespace Maba.Application.Features.Machines.Handlers;

public class GetMachinesByItemQueryHandler : IRequestHandler<GetMachinesByItemQuery, List<MachineDto>>
{
    private readonly IApplicationDbContext _context;

    public GetMachinesByItemQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MachineDto>> Handle(GetMachinesByItemQuery request, CancellationToken cancellationToken)
    {
        var item = await _context.Set<Domain.Catalog.Item>()
            .FirstOrDefaultAsync(i => i.Id == request.ItemId, cancellationToken);

        if (item == null)
        {
            throw new KeyNotFoundException("Item not found.");
        }

        var links = await _context.Set<ItemMachineLink>()
            .Include(l => l.Machine)
            .ThenInclude(m => m.Parts)
            .Where(l => l.ItemId == request.ItemId)
            .ToListAsync(cancellationToken);

        var machines = links.Select(l => l.Machine).DistinctBy(m => m.Id).ToList();

        return machines.Select(m => new MachineDto
        {
            Id = m.Id,
            NameEn = m.NameEn,
            NameAr = m.NameAr,
            Manufacturer = m.Manufacturer,
            Model = m.Model,
            YearFrom = m.YearFrom,
            YearTo = m.YearTo,
            ImageId = m.ImageId,
            ManualId = m.ManualId,
            WarrantyMonths = m.WarrantyMonths,
            Location = m.Location,
            PurchasePrice = m.PurchasePrice,
            PurchaseDate = m.PurchaseDate,
            Parts = m.Parts.Select(p => new MachinePartDto
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
            }).ToList(),
            CreatedAt = m.CreatedAt,
            UpdatedAt = m.UpdatedAt
        }).ToList();
    }
}

