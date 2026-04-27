using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Storage;

namespace Maba.Api.Controllers;

// ─── DTOs ────────────────────────────────────────────────────────────────────

public record StorageVariantDto(
    Guid Id, Guid ParentId,
    string? VariantLabel, string Sku,
    int Quantity, string Unit,
    string? Package, string? Value, string? ValueUnit, string? Tolerance,
    string? VoltageRating, string? CurrentRating, string? PowerRating,
    string? Notes, string? ImageUrl, string? DatasheetUrl,
    bool IsActive, bool IsPublishedToShop,
    DateTime CreatedAt, DateTime? UpdatedAt);

public record StorageParentDto(
    Guid Id, string Name, string? Description, string Category,
    string? Manufacturer, string? ImageUrl, string? DatasheetUrl,
    bool IsPublishedToShop, bool IsActive, int SortOrder,
    int VariantCount, int TotalQuantity,
    DateTime CreatedAt, DateTime? UpdatedAt,
    List<StorageVariantDto>? Variants = null);

public class UpsertStorageParentRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = "Other";
    public string? Manufacturer { get; set; }
    public string? ImageUrl { get; set; }
    public string? DatasheetUrl { get; set; }
    public bool IsPublishedToShop { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}

public class UpsertStorageVariantRequest
{
    public string? VariantLabel { get; set; }
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; } = 0;
    public string Unit { get; set; } = "pcs";
    public string? Package { get; set; }
    public string? Value { get; set; }
    public string? ValueUnit { get; set; }
    public string? Tolerance { get; set; }
    public string? VoltageRating { get; set; }
    public string? CurrentRating { get; set; }
    public string? PowerRating { get; set; }
    public string? Notes { get; set; }
    public string? ImageUrl { get; set; }
    public string? DatasheetUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsPublishedToShop { get; set; } = false;
}

// ─── Controller ──────────────────────────────────────────────────────────────

[ApiController]
[Route("api/v1/storage-inventory")]
[Authorize(Roles = "Admin,Manager")]
public class StorageInventoryController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public StorageInventoryController(IApplicationDbContext context)
    {
        _context = context;
    }

    // ── Parents ───────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<ActionResult<List<StorageParentDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] bool? publishedToShop,
        [FromQuery] bool? lowStock,
        [FromQuery] int? lowStockThreshold,
        CancellationToken ct)
    {
        var query = _context.Set<StorageParent>()
            .AsNoTracking()
            .Include(p => p.Variants)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(s) ||
                (p.Manufacturer != null && p.Manufacturer.ToLower().Contains(s)) ||
                p.Variants.Any(v =>
                    (v.Value != null && v.Value.ToLower().Contains(s)) ||
                    v.Sku.ToLower().Contains(s) ||
                    (v.Package != null && v.Package.ToLower().Contains(s)) ||
                    (v.VariantLabel != null && v.VariantLabel.ToLower().Contains(s))));
        }

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category == category);

        if (publishedToShop.HasValue)
            query = query.Where(p => p.IsPublishedToShop == publishedToShop.Value);

        var parents = await query.OrderBy(p => p.SortOrder).ThenBy(p => p.Name).ToListAsync(ct);

        var threshold = lowStockThreshold ?? 10;
        if (lowStock == true)
            parents = parents.Where(p => p.Variants.Any(v => v.IsActive && v.Quantity <= threshold)).ToList();

        return Ok(parents.Select(p => ToParentDto(p, includeVariants: false)).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StorageParentDto>> GetById(Guid id, CancellationToken ct)
    {
        var parent = await _context.Set<StorageParent>()
            .AsNoTracking()
            .Include(p => p.Variants.OrderBy(v => v.CreatedAt))
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (parent == null) return NotFound();
        return Ok(ToParentDto(parent, includeVariants: true));
    }

    [HttpPost]
    public async Task<ActionResult<StorageParentDto>> Create(
        [FromBody] UpsertStorageParentRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            return BadRequest(new { message = "Name is required." });

        var entity = new StorageParent
        {
            Name = req.Name.Trim(),
            Description = req.Description?.Trim(),
            Category = string.IsNullOrWhiteSpace(req.Category) ? "Other" : req.Category.Trim(),
            Manufacturer = req.Manufacturer?.Trim(),
            ImageUrl = req.ImageUrl?.Trim(),
            DatasheetUrl = req.DatasheetUrl?.Trim(),
            IsPublishedToShop = req.IsPublishedToShop,
            IsActive = req.IsActive,
            SortOrder = req.SortOrder
        };

        _context.Set<StorageParent>().Add(entity);
        await _context.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, ToParentDto(entity, false));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<StorageParentDto>> Update(
        Guid id, [FromBody] UpsertStorageParentRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            return BadRequest(new { message = "Name is required." });

        var entity = await _context.Set<StorageParent>()
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
        if (entity == null) return NotFound();

        entity.Name = req.Name.Trim();
        entity.Description = req.Description?.Trim();
        entity.Category = string.IsNullOrWhiteSpace(req.Category) ? "Other" : req.Category.Trim();
        entity.Manufacturer = req.Manufacturer?.Trim();
        entity.ImageUrl = req.ImageUrl?.Trim();
        entity.DatasheetUrl = req.DatasheetUrl?.Trim();
        entity.IsPublishedToShop = req.IsPublishedToShop;
        entity.IsActive = req.IsActive;
        entity.SortOrder = req.SortOrder;

        await _context.SaveChangesAsync(ct);
        return Ok(ToParentDto(entity, includeVariants: false));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        var entity = await _context.Set<StorageParent>().FirstOrDefaultAsync(p => p.Id == id, ct);
        if (entity == null) return NotFound();
        _context.Set<StorageParent>().Remove(entity);
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/toggle-shop")]
    public async Task<ActionResult<StorageParentDto>> ToggleShop(Guid id, CancellationToken ct)
    {
        var entity = await _context.Set<StorageParent>()
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
        if (entity == null) return NotFound();
        entity.IsPublishedToShop = !entity.IsPublishedToShop;
        await _context.SaveChangesAsync(ct);
        return Ok(ToParentDto(entity, false));
    }

    // ── Variants ──────────────────────────────────────────────────────────────

    [HttpGet("{parentId:guid}/variants")]
    public async Task<ActionResult<List<StorageVariantDto>>> GetVariants(Guid parentId, CancellationToken ct)
    {
        var exists = await _context.Set<StorageParent>().AsNoTracking().AnyAsync(p => p.Id == parentId, ct);
        if (!exists) return NotFound();

        var variants = await _context.Set<StorageVariant>()
            .AsNoTracking()
            .Where(v => v.ParentId == parentId)
            .OrderBy(v => v.CreatedAt)
            .ToListAsync(ct);

        return Ok(variants.Select(ToVariantDto).ToList());
    }

    [HttpPost("{parentId:guid}/variants")]
    public async Task<ActionResult<StorageVariantDto>> AddVariant(
        Guid parentId, [FromBody] UpsertStorageVariantRequest req, CancellationToken ct)
    {
        var parent = await _context.Set<StorageParent>().AsNoTracking().FirstOrDefaultAsync(p => p.Id == parentId, ct);
        if (parent == null) return NotFound();

        if (string.IsNullOrWhiteSpace(req.Sku))
            return BadRequest(new { message = "SKU is required." });

        var skuExists = await _context.Set<StorageVariant>().AnyAsync(v => v.Sku == req.Sku.Trim(), ct);
        if (skuExists) return Conflict(new { message = $"SKU '{req.Sku}' already exists." });

        var label = string.IsNullOrWhiteSpace(req.VariantLabel)
            ? BuildAutoLabel(parent.Name, req)
            : req.VariantLabel.Trim();

        var variant = new StorageVariant
        {
            ParentId = parentId,
            VariantLabel = label,
            Sku = req.Sku.Trim(),
            Quantity = Math.Max(0, req.Quantity),
            Unit = string.IsNullOrWhiteSpace(req.Unit) ? "pcs" : req.Unit.Trim(),
            Package = req.Package?.Trim(),
            Value = req.Value?.Trim(),
            ValueUnit = req.ValueUnit?.Trim(),
            Tolerance = req.Tolerance?.Trim(),
            VoltageRating = req.VoltageRating?.Trim(),
            CurrentRating = req.CurrentRating?.Trim(),
            PowerRating = req.PowerRating?.Trim(),
            Notes = req.Notes?.Trim(),
            ImageUrl = req.ImageUrl?.Trim(),
            DatasheetUrl = req.DatasheetUrl?.Trim(),
            IsActive = req.IsActive,
            IsPublishedToShop = req.IsPublishedToShop
        };

        _context.Set<StorageVariant>().Add(variant);
        await _context.SaveChangesAsync(ct);
        return Ok(ToVariantDto(variant));
    }

    [HttpPut("{parentId:guid}/variants/{variantId:guid}")]
    public async Task<ActionResult<StorageVariantDto>> UpdateVariant(
        Guid parentId, Guid variantId,
        [FromBody] UpsertStorageVariantRequest req, CancellationToken ct)
    {
        var variant = await _context.Set<StorageVariant>()
            .FirstOrDefaultAsync(v => v.Id == variantId && v.ParentId == parentId, ct);
        if (variant == null) return NotFound();

        if (string.IsNullOrWhiteSpace(req.Sku))
            return BadRequest(new { message = "SKU is required." });

        var skuExists = await _context.Set<StorageVariant>()
            .AnyAsync(v => v.Sku == req.Sku.Trim() && v.Id != variantId, ct);
        if (skuExists) return Conflict(new { message = $"SKU '{req.Sku}' already exists." });

        var parent = await _context.Set<StorageParent>().AsNoTracking().FirstOrDefaultAsync(p => p.Id == parentId, ct);
        var label = string.IsNullOrWhiteSpace(req.VariantLabel)
            ? BuildAutoLabel(parent?.Name ?? "", req)
            : req.VariantLabel.Trim();

        variant.VariantLabel = label;
        variant.Sku = req.Sku.Trim();
        variant.Quantity = Math.Max(0, req.Quantity);
        variant.Unit = string.IsNullOrWhiteSpace(req.Unit) ? "pcs" : req.Unit.Trim();
        variant.Package = req.Package?.Trim();
        variant.Value = req.Value?.Trim();
        variant.ValueUnit = req.ValueUnit?.Trim();
        variant.Tolerance = req.Tolerance?.Trim();
        variant.VoltageRating = req.VoltageRating?.Trim();
        variant.CurrentRating = req.CurrentRating?.Trim();
        variant.PowerRating = req.PowerRating?.Trim();
        variant.Notes = req.Notes?.Trim();
        variant.ImageUrl = req.ImageUrl?.Trim();
        variant.DatasheetUrl = req.DatasheetUrl?.Trim();
        variant.IsActive = req.IsActive;
        variant.IsPublishedToShop = req.IsPublishedToShop;

        await _context.SaveChangesAsync(ct);
        return Ok(ToVariantDto(variant));
    }

    [HttpDelete("{parentId:guid}/variants/{variantId:guid}")]
    public async Task<ActionResult> DeleteVariant(Guid parentId, Guid variantId, CancellationToken ct)
    {
        var variant = await _context.Set<StorageVariant>()
            .FirstOrDefaultAsync(v => v.Id == variantId && v.ParentId == parentId, ct);
        if (variant == null) return NotFound();
        _context.Set<StorageVariant>().Remove(variant);
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPatch("{parentId:guid}/variants/{variantId:guid}/quantity")]
    public async Task<ActionResult<StorageVariantDto>> AdjustQuantity(
        Guid parentId, Guid variantId,
        [FromBody] AdjustQtyRequest req, CancellationToken ct)
    {
        var variant = await _context.Set<StorageVariant>()
            .FirstOrDefaultAsync(v => v.Id == variantId && v.ParentId == parentId, ct);
        if (variant == null) return NotFound();

        variant.Quantity = Math.Max(0, req.Quantity);
        await _context.SaveChangesAsync(ct);
        return Ok(ToVariantDto(variant));
    }

    [HttpPatch("{parentId:guid}/variants/{variantId:guid}/toggle-shop")]
    public async Task<ActionResult<StorageVariantDto>> ToggleVariantShop(
        Guid parentId, Guid variantId, CancellationToken ct)
    {
        var variant = await _context.Set<StorageVariant>()
            .FirstOrDefaultAsync(v => v.Id == variantId && v.ParentId == parentId, ct);
        if (variant == null) return NotFound();
        variant.IsPublishedToShop = !variant.IsPublishedToShop;
        await _context.SaveChangesAsync(ct);
        return Ok(ToVariantDto(variant));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static StorageParentDto ToParentDto(StorageParent p, bool includeVariants)
    {
        var activeVariants = p.Variants.Where(v => v.IsActive).ToList();
        return new StorageParentDto(
            p.Id, p.Name, p.Description, p.Category,
            p.Manufacturer, p.ImageUrl, p.DatasheetUrl,
            p.IsPublishedToShop, p.IsActive, p.SortOrder,
            VariantCount: activeVariants.Count,
            TotalQuantity: activeVariants.Sum(v => v.Quantity),
            p.CreatedAt, p.UpdatedAt,
            Variants: includeVariants ? p.Variants.OrderBy(v => v.CreatedAt).Select(ToVariantDto).ToList() : null);
    }

    private static StorageVariantDto ToVariantDto(StorageVariant v) =>
        new(v.Id, v.ParentId, v.VariantLabel, v.Sku, v.Quantity, v.Unit,
            v.Package, v.Value, v.ValueUnit, v.Tolerance,
            v.VoltageRating, v.CurrentRating, v.PowerRating,
            v.Notes, v.ImageUrl, v.DatasheetUrl,
            v.IsActive, v.IsPublishedToShop, v.CreatedAt, v.UpdatedAt);

    private static string BuildAutoLabel(string parentName, UpsertStorageVariantRequest r)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(r.Value) && !string.IsNullOrWhiteSpace(r.ValueUnit))
            parts.Add($"{r.Value}{r.ValueUnit}");
        else if (!string.IsNullOrWhiteSpace(r.Value))
            parts.Add(r.Value);
        if (!string.IsNullOrWhiteSpace(r.Package)) parts.Add(r.Package);
        if (!string.IsNullOrWhiteSpace(r.Tolerance)) parts.Add(r.Tolerance);
        return parts.Count > 0 ? string.Join(" ", parts) : parentName;
    }
}

public record AdjustQtyRequest(int Quantity);
