using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Machines.Queries;
using Maba.Application.Features.Machines.DTOs;
using Maba.Domain.Machines;

namespace Maba.Application.Features.Machines.Queries;

public class GetAllMachinePartsQueryHandler : IRequestHandler<GetAllMachinePartsQuery, GetAllMachinePartsResult>
{
    private readonly IApplicationDbContext _context;

    public GetAllMachinePartsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetAllMachinePartsResult> Handle(GetAllMachinePartsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<MachinePart>().AsQueryable();

        if (request.MachineId.HasValue)
            query = query.Where(p => p.MachineId == request.MachineId.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim().ToLower();
            query = query.Where(p =>
                p.PartNameEn.ToLower().Contains(s) ||
                p.PartNameAr.ToLower().Contains(s) ||
                (p.PartCode != null && p.PartCode.ToLower().Contains(s)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var parts = await query
            .OrderBy(p => p.PartNameEn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new MachinePartDto
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
            })
            .ToListAsync(cancellationToken);

        return new GetAllMachinePartsResult
        {
            Items = parts,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }
}
