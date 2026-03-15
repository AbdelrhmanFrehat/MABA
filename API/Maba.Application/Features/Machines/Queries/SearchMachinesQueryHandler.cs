using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Common.Models;
using Maba.Application.Features.Machines.Queries;
using Maba.Application.Features.Machines.DTOs;
using Maba.Domain.Machines;

namespace Maba.Application.Features.Machines.Handlers;

public class SearchMachinesQueryHandler : IRequestHandler<SearchMachinesQuery, PagedResult<MachineDto>>
{
    private readonly IApplicationDbContext _context;

    public SearchMachinesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<MachineDto>> Handle(SearchMachinesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<Machine>()
            .Include(m => m.Parts)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(m =>
                m.NameEn.ToLower().Contains(searchTerm) ||
                m.NameAr.ToLower().Contains(searchTerm) ||
                (m.Manufacturer != null && m.Manufacturer.ToLower().Contains(searchTerm)) ||
                (m.Model != null && m.Model.ToLower().Contains(searchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(request.Manufacturer))
        {
            query = query.Where(m => m.Manufacturer != null && m.Manufacturer.ToLower().Contains(request.Manufacturer.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(request.Model))
        {
            query = query.Where(m => m.Model != null && m.Model.ToLower().Contains(request.Model.ToLower()));
        }

        if (request.YearFrom.HasValue)
        {
            query = query.Where(m => m.YearFrom == null || m.YearFrom <= request.YearFrom.Value);
        }

        if (request.YearTo.HasValue)
        {
            query = query.Where(m => m.YearTo == null || m.YearTo >= request.YearTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Location))
        {
            query = query.Where(m => m.Location != null && m.Location.ToLower().Contains(request.Location.ToLower()));
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending
                ? query.OrderByDescending(m => m.NameEn)
                : query.OrderBy(m => m.NameEn),
            "manufacturer" => request.SortDescending
                ? query.OrderByDescending(m => m.Manufacturer)
                : query.OrderBy(m => m.Manufacturer),
            "purchasedate" => request.SortDescending
                ? query.OrderByDescending(m => m.PurchaseDate)
                : query.OrderBy(m => m.PurchaseDate),
            _ => query.OrderBy(m => m.NameEn)
        };

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var machines = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var machinesDto = machines.Select(m => new MachineDto
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

        return new PagedResult<MachineDto>
        {
            Items = machinesDto,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

