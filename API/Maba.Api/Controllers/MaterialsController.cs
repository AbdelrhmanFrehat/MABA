using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Maba.Application.Features.Printing.Materials.Commands;
using Maba.Application.Features.Printing.DTOs;
using Maba.Application.Features.Printing.Materials.Queries;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Printing;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class MaterialsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _context;

    public MaterialsController(IMediator mediator, IApplicationDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<MaterialDto>>> GetAllMaterials()
    {
        var query = new GetAllMaterialsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<MaterialDto>> GetMaterialById(Guid id)
    {
        var query = new GetMaterialByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        
        if (result == null)
        {
            return NotFound();
        }
        
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<MaterialDto>> CreateMaterial([FromBody] CreateMaterialCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetMaterialById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<MaterialDto>> UpdateMaterial(Guid id, [FromBody] UpdateMaterialCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("ID mismatch");
        }
        
        var result = await _mediator.Send(command);
        
        if (result == null)
        {
            return NotFound();
        }
        
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMaterial(Guid id)
    {
        var command = new DeleteMaterialCommand { Id = id };
        var result = await _mediator.Send(command);
        
        if (!result)
        {
            return NotFound();
        }
        
        return NoContent();
    }

    // ========== Material Colors ==========

    [HttpGet("{materialId}/colors")]
    [AllowAnonymous]
    public async Task<ActionResult<List<MaterialColorDto>>> GetMaterialColors(Guid materialId)
    {
        // Only return colors that have at least one active spool with remaining stock
        var colorIdsWithStock = await _context.Set<FilamentSpool>()
            .Where(s => s.MaterialId == materialId && s.IsActive && s.RemainingWeightGrams > 0 && s.MaterialColorId != null)
            .Select(s => s.MaterialColorId!.Value)
            .Distinct()
            .ToListAsync();

        var colors = await _context.Set<MaterialColor>()
            .Where(c => c.MaterialId == materialId && c.IsActive && colorIdsWithStock.Contains(c.Id))
            .OrderBy(c => c.SortOrder)
            .Select(c => new MaterialColorDto
            {
                Id = c.Id,
                MaterialId = c.MaterialId,
                NameEn = c.NameEn,
                NameAr = c.NameAr,
                HexCode = c.HexCode,
                IsActive = c.IsActive,
                SortOrder = c.SortOrder,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync();

        return Ok(colors);
    }

    [HttpGet("{materialId}/colors/all")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<List<MaterialColorDto>>> GetAllMaterialColors(Guid materialId)
    {
        var colors = await _context.Set<MaterialColor>()
            .Where(c => c.MaterialId == materialId)
            .OrderBy(c => c.SortOrder)
            .Select(c => new MaterialColorDto
            {
                Id = c.Id,
                MaterialId = c.MaterialId,
                NameEn = c.NameEn,
                NameAr = c.NameAr,
                HexCode = c.HexCode,
                IsActive = c.IsActive,
                SortOrder = c.SortOrder,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync();

        return Ok(colors);
    }

    [HttpPost("{materialId}/colors")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<MaterialColorDto>> CreateMaterialColor(Guid materialId, [FromBody] CreateMaterialColorDto dto)
    {
        var materialExists = await _context.Set<Material>().AnyAsync(m => m.Id == materialId);
        if (!materialExists)
        {
            return NotFound("Material not found");
        }

        var color = new MaterialColor
        {
            Id = Guid.NewGuid(),
            MaterialId = materialId,
            NameEn = dto.NameEn,
            NameAr = dto.NameAr,
            HexCode = dto.HexCode.StartsWith("#") ? dto.HexCode : $"#{dto.HexCode}",
            IsActive = dto.IsActive,
            SortOrder = dto.SortOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Set<MaterialColor>().Add(color);
        await _context.SaveChangesAsync(CancellationToken.None);

        return CreatedAtAction(nameof(GetMaterialColors), new { materialId }, new MaterialColorDto
        {
            Id = color.Id,
            MaterialId = color.MaterialId,
            NameEn = color.NameEn,
            NameAr = color.NameAr,
            HexCode = color.HexCode,
            IsActive = color.IsActive,
            SortOrder = color.SortOrder,
            CreatedAt = color.CreatedAt,
            UpdatedAt = color.UpdatedAt
        });
    }

    [HttpPut("{materialId}/colors/{colorId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<MaterialColorDto>> UpdateMaterialColor(Guid materialId, Guid colorId, [FromBody] UpdateMaterialColorDto dto)
    {
        var color = await _context.Set<MaterialColor>()
            .FirstOrDefaultAsync(c => c.Id == colorId && c.MaterialId == materialId);

        if (color == null)
        {
            return NotFound();
        }

        color.NameEn = dto.NameEn;
        color.NameAr = dto.NameAr;
        color.HexCode = dto.HexCode.StartsWith("#") ? dto.HexCode : $"#{dto.HexCode}";
        color.IsActive = dto.IsActive;
        color.SortOrder = dto.SortOrder;
        color.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(CancellationToken.None);

        return Ok(new MaterialColorDto
        {
            Id = color.Id,
            MaterialId = color.MaterialId,
            NameEn = color.NameEn,
            NameAr = color.NameAr,
            HexCode = color.HexCode,
            IsActive = color.IsActive,
            SortOrder = color.SortOrder,
            CreatedAt = color.CreatedAt,
            UpdatedAt = color.UpdatedAt
        });
    }

    [HttpDelete("{materialId}/colors/{colorId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> DeleteMaterialColor(Guid materialId, Guid colorId)
    {
        var color = await _context.Set<MaterialColor>()
            .FirstOrDefaultAsync(c => c.Id == colorId && c.MaterialId == materialId);

        if (color == null)
        {
            return NotFound();
        }

        _context.Set<MaterialColor>().Remove(color);
        await _context.SaveChangesAsync(CancellationToken.None);

        return NoContent();
    }
}

