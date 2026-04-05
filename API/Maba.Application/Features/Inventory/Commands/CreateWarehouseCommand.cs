using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.BusinessInventory.DTOs;
using Maba.Domain.Inventory;

namespace Maba.Application.Features.BusinessInventory.Commands;

public class CreateWarehouseCommand : IRequest<WarehouseDto>
{
    public string Code { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CreateWarehouseCommandHandler : IRequestHandler<CreateWarehouseCommand, WarehouseDto>
{
    private readonly IApplicationDbContext _context;

    public CreateWarehouseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WarehouseDto> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
    {
        if (request.IsDefault)
        {
            var defaultWarehouses = await _context.Set<Warehouse>()
                .Where(x => x.IsDefault)
                .ToListAsync(cancellationToken);

            foreach (var warehouse in defaultWarehouses)
            {
                warehouse.IsDefault = false;
            }
        }

        var entity = new Warehouse
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            Address = request.Address,
            IsDefault = request.IsDefault,
            IsActive = request.IsActive
        };

        _context.Set<Warehouse>().Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new WarehouseDto
        {
            Id = entity.Id,
            Code = entity.Code,
            NameEn = entity.NameEn,
            NameAr = entity.NameAr,
            Address = entity.Address,
            IsDefault = entity.IsDefault,
            IsActive = entity.IsActive
        };
    }
}
