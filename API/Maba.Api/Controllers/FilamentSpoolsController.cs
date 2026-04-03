using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Printing.FilamentSpools;
using Maba.Domain.Printing;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/filament-spools")]
[Authorize(Roles = "Admin,Manager")]
public class FilamentSpoolsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public FilamentSpoolsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<FilamentSpoolDto>>> GetAll(CancellationToken cancellationToken)
    {
        var rows = await _context.Set<FilamentSpool>()
            .AsNoTracking()
            .Include(s => s.Material)
            .Include(s => s.MaterialColor)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
        return Ok(rows.Select(ToDto).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FilamentSpoolDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var s = await _context.Set<FilamentSpool>()
            .AsNoTracking()
            .Include(x => x.Material)
            .Include(x => x.MaterialColor)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (s == null)
        {
            return NotFound();
        }

        return Ok(ToDto(s));
    }

    [HttpPost]
    public async Task<ActionResult<FilamentSpoolDto>> Create([FromBody] CreateFilamentSpoolDto dto, CancellationToken cancellationToken)
    {
        if (dto.InitialWeightGrams <= 0)
        {
            return BadRequest("InitialWeightGrams must be greater than 0.");
        }

        var material = await _context.Set<Material>().FindAsync(new object[] { dto.MaterialId }, cancellationToken);
        if (material == null)
        {
            return BadRequest("Material not found.");
        }

        if (dto.MaterialColorId.HasValue)
        {
            var color = await _context.Set<MaterialColor>()
                .FirstOrDefaultAsync(c => c.Id == dto.MaterialColorId.Value && c.MaterialId == dto.MaterialId, cancellationToken);
            if (color == null)
            {
                return BadRequest("Material color not found for this material.");
            }
        }

        var initial = dto.InitialWeightGrams;
        var entity = new FilamentSpool
        {
            MaterialId = dto.MaterialId,
            MaterialColorId = dto.MaterialColorId,
            Name = string.IsNullOrWhiteSpace(dto.Name) ? null : dto.Name.Trim(),
            InitialWeightGrams = initial,
            RemainingWeightGrams = initial,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<FilamentSpool>().Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        var created = await _context.Set<FilamentSpool>()
            .AsNoTracking()
            .Include(s => s.Material)
            .Include(s => s.MaterialColor)
            .FirstAsync(s => s.Id == entity.Id, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToDto(created));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<FilamentSpoolDto>> Update(Guid id, [FromBody] UpdateFilamentSpoolDto dto, CancellationToken cancellationToken)
    {
        if (dto.RemainingWeightGrams < 0)
        {
            return BadRequest("RemainingWeightGrams must be greater than or equal to 0.");
        }

        var entity = await _context.Set<FilamentSpool>()
            .Include(s => s.Material)
            .Include(s => s.MaterialColor)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (entity == null)
        {
            return NotFound();
        }

        entity.Name = string.IsNullOrWhiteSpace(dto.Name) ? null : dto.Name.Trim();
        entity.RemainingWeightGrams = dto.RemainingWeightGrams;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(ToDto(entity));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<FilamentSpool>().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (entity == null)
        {
            return NotFound();
        }

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static FilamentSpoolDto ToDto(FilamentSpool s) => new()
    {
        Id = s.Id,
        MaterialId = s.MaterialId,
        MaterialNameEn = s.Material.NameEn,
        MaterialNameAr = s.Material.NameAr,
        MaterialColorId = s.MaterialColorId,
        ColorNameEn = s.MaterialColor?.NameEn,
        ColorNameAr = s.MaterialColor?.NameAr,
        Name = s.Name,
        InitialWeightGrams = s.InitialWeightGrams,
        RemainingWeightGrams = s.RemainingWeightGrams,
        IsActive = s.IsActive,
        CreatedAt = s.CreatedAt
    };
}
