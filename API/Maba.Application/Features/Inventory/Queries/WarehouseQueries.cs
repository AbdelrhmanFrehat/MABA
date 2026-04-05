using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.BusinessInventory.DTOs;
using Maba.Domain.Inventory;

namespace Maba.Application.Features.BusinessInventory.Queries;

public class GetWarehousesQuery : IRequest<List<WarehouseDto>>
{
}

public class GetWarehousesQueryHandler : IRequestHandler<GetWarehousesQuery, List<WarehouseDto>>
{
    private readonly IApplicationDbContext _context;

    public GetWarehousesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<WarehouseDto>> Handle(GetWarehousesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Set<Warehouse>()
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.Code)
            .Select(x => new WarehouseDto
            {
                Id = x.Id,
                Code = x.Code,
                NameEn = x.NameEn,
                NameAr = x.NameAr,
                Address = x.Address,
                IsDefault = x.IsDefault,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}
