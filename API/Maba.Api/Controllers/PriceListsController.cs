using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Lookups;
using Maba.Domain.Pricing;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/price-lists")]
[Authorize]
public class PriceListsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public PriceListsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PriceListDto>>> GetPriceLists(CancellationToken cancellationToken)
    {
        var rows = await _context.Set<PriceList>()
            .Include(x => x.PriceListType)
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.NameEn)
            .Select(x => new PriceListDto
            {
                Id = x.Id,
                NameEn = x.NameEn,
                NameAr = x.NameAr,
                PriceListTypeId = x.PriceListTypeId,
                PriceListTypeName = x.PriceListType.NameEn,
                Currency = x.Currency,
                IsDefault = x.IsDefault,
                IsActive = x.IsActive,
                ValidFrom = x.ValidFrom,
                ValidTo = x.ValidTo,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(rows);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PriceListDto>> GetPriceList(Guid id, CancellationToken cancellationToken)
    {
        var row = await _context.Set<PriceList>()
            .Include(x => x.PriceListType)
            .Where(x => x.Id == id)
            .Select(x => new PriceListDto
            {
                Id = x.Id,
                NameEn = x.NameEn,
                NameAr = x.NameAr,
                PriceListTypeId = x.PriceListTypeId,
                PriceListTypeName = x.PriceListType.NameEn,
                Currency = x.Currency,
                IsDefault = x.IsDefault,
                IsActive = x.IsActive,
                ValidFrom = x.ValidFrom,
                ValidTo = x.ValidTo,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        return row is null ? NotFound() : Ok(row);
    }

    [HttpPost]
    public async Task<ActionResult<PriceListDto>> CreatePriceList([FromBody] UpsertPriceListRequest request, CancellationToken cancellationToken)
    {
        await EnsurePriceListTypeExists(request.PriceListTypeId, cancellationToken);

        if (request.IsDefault)
        {
            await ClearDefaultPriceLists(cancellationToken);
        }

        var entity = new PriceList
        {
            Id = Guid.NewGuid(),
            NameEn = request.NameEn.Trim(),
            NameAr = request.NameAr.Trim(),
            PriceListTypeId = request.PriceListTypeId,
            Currency = string.IsNullOrWhiteSpace(request.Currency) ? "ILS" : request.Currency.Trim().ToUpperInvariant(),
            IsDefault = request.IsDefault,
            IsActive = request.IsActive,
            ValidFrom = request.ValidFrom,
            ValidTo = request.ValidTo
        };

        _context.Set<PriceList>().Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        var lookupName = await _context.Set<LookupValue>()
            .Where(x => x.Id == entity.PriceListTypeId)
            .Select(x => x.NameEn)
            .FirstAsync(cancellationToken);

        return CreatedAtAction(nameof(GetPriceList), new { id = entity.Id }, ToDto(entity, lookupName));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PriceListDto>> UpdatePriceList(Guid id, [FromBody] UpsertPriceListRequest request, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<PriceList>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        await EnsurePriceListTypeExists(request.PriceListTypeId, cancellationToken);

        if (request.IsDefault && !entity.IsDefault)
        {
            await ClearDefaultPriceLists(cancellationToken);
        }

        entity.NameEn = request.NameEn.Trim();
        entity.NameAr = request.NameAr.Trim();
        entity.PriceListTypeId = request.PriceListTypeId;
        entity.Currency = string.IsNullOrWhiteSpace(request.Currency) ? "ILS" : request.Currency.Trim().ToUpperInvariant();
        entity.IsDefault = request.IsDefault;
        entity.IsActive = request.IsActive;
        entity.ValidFrom = request.ValidFrom;
        entity.ValidTo = request.ValidTo;

        await _context.SaveChangesAsync(cancellationToken);

        var lookupName = await _context.Set<LookupValue>()
            .Where(x => x.Id == entity.PriceListTypeId)
            .Select(x => x.NameEn)
            .FirstAsync(cancellationToken);

        return Ok(ToDto(entity, lookupName));
    }

    private async Task EnsurePriceListTypeExists(Guid priceListTypeId, CancellationToken cancellationToken)
    {
        var exists = await _context.Set<LookupValue>()
            .AnyAsync(x => x.Id == priceListTypeId && x.LookupType.Key == "PriceListType", cancellationToken);

        if (!exists)
        {
            throw new KeyNotFoundException("Price list type not found.");
        }
    }

    private async Task ClearDefaultPriceLists(CancellationToken cancellationToken)
    {
        var currentDefaults = await _context.Set<PriceList>()
            .Where(x => x.IsDefault)
            .ToListAsync(cancellationToken);

        foreach (var row in currentDefaults)
        {
            row.IsDefault = false;
        }
    }

    private static PriceListDto ToDto(PriceList entity, string priceListTypeName)
    {
        return new PriceListDto
        {
            Id = entity.Id,
            NameEn = entity.NameEn,
            NameAr = entity.NameAr,
            PriceListTypeId = entity.PriceListTypeId,
            PriceListTypeName = priceListTypeName,
            Currency = entity.Currency,
            IsDefault = entity.IsDefault,
            IsActive = entity.IsActive,
            ValidFrom = entity.ValidFrom,
            ValidTo = entity.ValidTo,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public class UpsertPriceListRequest
    {
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public Guid PriceListTypeId { get; set; }
        public string? Currency { get; set; } = "ILS";
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
    }

    public class PriceListDto
    {
        public Guid Id { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public Guid PriceListTypeId { get; set; }
        public string PriceListTypeName { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
