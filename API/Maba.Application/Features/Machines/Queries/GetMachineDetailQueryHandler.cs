using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Machines.Queries;
using Maba.Application.Features.Machines.DTOs;
using Maba.Domain.Machines;

namespace Maba.Application.Features.Machines.Handlers;

public class GetMachineDetailQueryHandler : IRequestHandler<GetMachineDetailQuery, MachineDetailDto>
{
    private readonly IApplicationDbContext _context;

    public GetMachineDetailQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MachineDetailDto> Handle(GetMachineDetailQuery request, CancellationToken cancellationToken)
    {
        var machine = await _context.Set<Machine>()
            .Include(m => m.Parts)
            .Include(m => m.ItemMachineLinks)
            .FirstOrDefaultAsync(m => m.Id == request.MachineId, cancellationToken);

        if (machine == null)
        {
            throw new KeyNotFoundException("Machine not found.");
        }

        return new MachineDetailDto
        {
            Id = machine.Id,
            NameEn = machine.NameEn,
            NameAr = machine.NameAr,
            Manufacturer = machine.Manufacturer,
            Model = machine.Model,
            YearFrom = machine.YearFrom,
            YearTo = machine.YearTo,
            ImageId = machine.ImageId,
            ManualId = machine.ManualId,
            WarrantyMonths = machine.WarrantyMonths,
            Location = machine.Location,
            PurchasePrice = machine.PurchasePrice,
            PurchaseDate = machine.PurchaseDate,
            Parts = machine.Parts.Select(p => new MachinePartDto
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
            PartsCount = machine.Parts.Count,
            ItemsCount = machine.ItemMachineLinks.Count,
            CreatedAt = machine.CreatedAt,
            UpdatedAt = machine.UpdatedAt
        };
    }
}

