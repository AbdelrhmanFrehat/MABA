using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Software;
using System.Security.Cryptography;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/software")]
public class SoftwareController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;

    public SoftwareController(IApplicationDbContext context, IFileStorageService fileStorageService)
    {
        _context = context;
        _fileStorageService = fileStorageService;
    }

    #region Public Endpoints

    /// <summary>
    /// Get all active software products with their latest version
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<SoftwareProductListDto>>> GetProducts(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] string? os)
    {
        var query = _context.Set<SoftwareProduct>()
            .Where(p => p.IsActive)
            .Include(p => p.Releases.Where(r => r.IsActive).OrderByDescending(r => r.ReleaseDate).Take(1))
                .ThenInclude(r => r.Files.Where(f => f.IsActive))
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(p => 
                p.NameEn.ToLower().Contains(searchLower) ||
                p.NameAr.ToLower().Contains(searchLower) ||
                (p.SummaryEn != null && p.SummaryEn.ToLower().Contains(searchLower)));
        }

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(p => p.Category == category);
        }

        var products = await query
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.NameEn)
            .Select(p => new SoftwareProductListDto
            {
                Id = p.Id,
                Slug = p.Slug,
                NameEn = p.NameEn,
                NameAr = p.NameAr,
                SummaryEn = p.SummaryEn,
                SummaryAr = p.SummaryAr,
                Category = p.Category,
                IconKey = p.IconKey,
                LatestVersion = p.Releases.FirstOrDefault() != null ? p.Releases.First().Version : null,
                LatestReleaseDate = p.Releases.FirstOrDefault() != null ? p.Releases.First().ReleaseDate : null,
                LatestReleaseStatus = p.Releases.FirstOrDefault() != null ? p.Releases.First().Status.ToString() : null,
                DownloadCount = p.Releases.SelectMany(r => r.Files).Sum(f => f.DownloadCount)
            })
            .ToListAsync();

        // Filter by OS if specified
        if (!string.IsNullOrEmpty(os) && Enum.TryParse<SoftwareFileOs>(os, true, out var osEnum))
        {
            var productIds = await _context.Set<SoftwareProduct>()
                .Where(p => p.IsActive && p.Releases.Any(r => r.IsActive && r.Files.Any(f => f.IsActive && f.Os == osEnum)))
                .Select(p => p.Id)
                .ToListAsync();
            products = products.Where(p => productIds.Contains(p.Id)).ToList();
        }

        return Ok(products);
    }

    /// <summary>
    /// Get all categories
    /// </summary>
    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<ActionResult<List<string>>> GetCategories()
    {
        var categories = await _context.Set<SoftwareProduct>()
            .Where(p => p.IsActive && p.Category != null)
            .Select(p => p.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        return Ok(categories);
    }

    /// <summary>
    /// Get a software product by slug with all releases
    /// </summary>
    [HttpGet("{slug}")]
    [AllowAnonymous]
    public async Task<ActionResult<SoftwareProductDetailDto>> GetProductBySlug(string slug)
    {
        var product = await _context.Set<SoftwareProduct>()
            .Include(p => p.Releases.Where(r => r.IsActive).OrderByDescending(r => r.ReleaseDate))
                .ThenInclude(r => r.Files.Where(f => f.IsActive))
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);

        if (product == null)
        {
            return NotFound();
        }

        return Ok(new SoftwareProductDetailDto
        {
            Id = product.Id,
            Slug = product.Slug,
            NameEn = product.NameEn,
            NameAr = product.NameAr,
            SummaryEn = product.SummaryEn,
            SummaryAr = product.SummaryAr,
            DescriptionEn = product.DescriptionEn,
            DescriptionAr = product.DescriptionAr,
            Category = product.Category,
            IconKey = product.IconKey,
            LicenseEn = product.LicenseEn,
            LicenseAr = product.LicenseAr,
            Releases = product.Releases.Select(r => new SoftwareReleaseDto
            {
                Id = r.Id,
                Version = r.Version,
                ReleaseDate = r.ReleaseDate,
                Status = r.Status.ToString(),
                ChangelogEn = r.ChangelogEn,
                ChangelogAr = r.ChangelogAr,
                RequirementsEn = r.RequirementsEn,
                RequirementsAr = r.RequirementsAr,
                Files = r.Files.Select(f => new SoftwareFileDto
                {
                    Id = f.Id,
                    Os = f.Os.ToString(),
                    Arch = f.Arch.ToString(),
                    FileType = f.FileType.ToString(),
                    FileName = f.FileName,
                    FileSizeBytes = f.FileSizeBytes,
                    Sha256 = f.Sha256,
                    DownloadCount = f.DownloadCount
                }).ToList()
            }).ToList()
        });
    }

    /// <summary>
    /// Download a software file (increments download count)
    /// </summary>
    [HttpPost("download/{fileId}")]
    [AllowAnonymous]
    public async Task<IActionResult> DownloadFile(Guid fileId)
    {
        var file = await _context.Set<SoftwareFile>()
            .Include(f => f.Release)
                .ThenInclude(r => r.Product)
            .FirstOrDefaultAsync(f => f.Id == fileId && f.IsActive);

        if (file == null)
        {
            return NotFound();
        }

        // Increment download count
        file.DownloadCount++;
        await _context.SaveChangesAsync(CancellationToken.None);

        // Get file stream
        var fileStream = await _fileStorageService.GetFileAsync(file.StoredPath);
        if (fileStream == null)
        {
            return NotFound("File not found on storage");
        }

        return File(fileStream, "application/octet-stream", file.FileName);
    }

    #endregion

    #region Admin Endpoints

    /// <summary>
    /// Get all software products (admin)
    /// </summary>
    [HttpGet("admin/products")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<List<SoftwareProductListDto>>> GetAllProductsAdmin()
    {
        var products = await _context.Set<SoftwareProduct>()
            .Include(p => p.Releases)
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.NameEn)
            .Select(p => new SoftwareProductListDto
            {
                Id = p.Id,
                Slug = p.Slug,
                NameEn = p.NameEn,
                NameAr = p.NameAr,
                SummaryEn = p.SummaryEn,
                SummaryAr = p.SummaryAr,
                Category = p.Category,
                IconKey = p.IconKey,
                IsActive = p.IsActive,
                SortOrder = p.SortOrder,
                ReleasesCount = p.Releases.Count,
                LatestVersion = p.Releases.OrderByDescending(r => r.ReleaseDate).FirstOrDefault() != null 
                    ? p.Releases.OrderByDescending(r => r.ReleaseDate).First().Version : null,
                DownloadCount = p.Releases.SelectMany(r => r.Files).Sum(f => f.DownloadCount)
            })
            .ToListAsync();

        return Ok(products);
    }

    /// <summary>
    /// Create a software product
    /// </summary>
    [HttpPost("admin/products")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<SoftwareProductListDto>> CreateProduct([FromBody] CreateSoftwareProductDto dto)
    {
        // Validate unique slug
        var existingSlug = await _context.Set<SoftwareProduct>()
            .AnyAsync(p => p.Slug == dto.Slug);
        if (existingSlug)
        {
            return BadRequest("A product with this slug already exists");
        }

        var product = new SoftwareProduct
        {
            Id = Guid.NewGuid(),
            Slug = dto.Slug,
            NameEn = dto.NameEn,
            NameAr = dto.NameAr,
            SummaryEn = dto.SummaryEn,
            SummaryAr = dto.SummaryAr,
            DescriptionEn = dto.DescriptionEn,
            DescriptionAr = dto.DescriptionAr,
            Category = dto.Category,
            IconKey = dto.IconKey,
            LicenseEn = dto.LicenseEn,
            LicenseAr = dto.LicenseAr,
            IsActive = dto.IsActive,
            SortOrder = dto.SortOrder
        };

        _context.Set<SoftwareProduct>().Add(product);
        await _context.SaveChangesAsync(CancellationToken.None);

        return CreatedAtAction(nameof(GetProductBySlug), new { slug = product.Slug }, new SoftwareProductListDto
        {
            Id = product.Id,
            Slug = product.Slug,
            NameEn = product.NameEn,
            NameAr = product.NameAr,
            SummaryEn = product.SummaryEn,
            SummaryAr = product.SummaryAr,
            Category = product.Category,
            IconKey = product.IconKey,
            IsActive = product.IsActive,
            SortOrder = product.SortOrder
        });
    }

    /// <summary>
    /// Update a software product
    /// </summary>
    [HttpPut("admin/products/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<SoftwareProductListDto>> UpdateProduct(Guid id, [FromBody] CreateSoftwareProductDto dto)
    {
        var product = await _context.Set<SoftwareProduct>().FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        // Validate unique slug (if changed)
        if (product.Slug != dto.Slug)
        {
            var existingSlug = await _context.Set<SoftwareProduct>()
                .AnyAsync(p => p.Slug == dto.Slug && p.Id != id);
            if (existingSlug)
            {
                return BadRequest("A product with this slug already exists");
            }
        }

        product.Slug = dto.Slug;
        product.NameEn = dto.NameEn;
        product.NameAr = dto.NameAr;
        product.SummaryEn = dto.SummaryEn;
        product.SummaryAr = dto.SummaryAr;
        product.DescriptionEn = dto.DescriptionEn;
        product.DescriptionAr = dto.DescriptionAr;
        product.Category = dto.Category;
        product.IconKey = dto.IconKey;
        product.LicenseEn = dto.LicenseEn;
        product.LicenseAr = dto.LicenseAr;
        product.IsActive = dto.IsActive;
        product.SortOrder = dto.SortOrder;

        await _context.SaveChangesAsync(CancellationToken.None);

        return Ok(new SoftwareProductListDto
        {
            Id = product.Id,
            Slug = product.Slug,
            NameEn = product.NameEn,
            NameAr = product.NameAr,
            SummaryEn = product.SummaryEn,
            SummaryAr = product.SummaryAr,
            Category = product.Category,
            IconKey = product.IconKey,
            IsActive = product.IsActive,
            SortOrder = product.SortOrder
        });
    }

    /// <summary>
    /// Delete a software product
    /// </summary>
    [HttpDelete("admin/products/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var product = await _context.Set<SoftwareProduct>()
            .Include(p => p.Releases)
                .ThenInclude(r => r.Files)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        // Delete all associated files from storage
        foreach (var release in product.Releases)
        {
            foreach (var file in release.Files)
            {
                await _fileStorageService.DeleteFileAsync(file.StoredPath);
            }
        }

        _context.Set<SoftwareProduct>().Remove(product);
        await _context.SaveChangesAsync(CancellationToken.None);

        return NoContent();
    }

    /// <summary>
    /// Get releases for a product
    /// </summary>
    [HttpGet("admin/products/{productId}/releases")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<List<SoftwareReleaseDto>>> GetReleases(Guid productId)
    {
        var releases = await _context.Set<SoftwareRelease>()
            .Where(r => r.ProductId == productId)
            .Include(r => r.Files)
            .OrderByDescending(r => r.ReleaseDate)
            .Select(r => new SoftwareReleaseDto
            {
                Id = r.Id,
                Version = r.Version,
                ReleaseDate = r.ReleaseDate,
                Status = r.Status.ToString(),
                ChangelogEn = r.ChangelogEn,
                ChangelogAr = r.ChangelogAr,
                RequirementsEn = r.RequirementsEn,
                RequirementsAr = r.RequirementsAr,
                IsActive = r.IsActive,
                Files = r.Files.Select(f => new SoftwareFileDto
                {
                    Id = f.Id,
                    Os = f.Os.ToString(),
                    Arch = f.Arch.ToString(),
                    FileType = f.FileType.ToString(),
                    FileName = f.FileName,
                    FileSizeBytes = f.FileSizeBytes,
                    Sha256 = f.Sha256,
                    DownloadCount = f.DownloadCount,
                    IsActive = f.IsActive
                }).ToList()
            })
            .ToListAsync();

        return Ok(releases);
    }

    /// <summary>
    /// Create a release
    /// </summary>
    [HttpPost("admin/products/{productId}/releases")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<SoftwareReleaseDto>> CreateRelease(Guid productId, [FromBody] CreateSoftwareReleaseDto dto)
    {
        var product = await _context.Set<SoftwareProduct>().FindAsync(productId);
        if (product == null)
        {
            return NotFound("Product not found");
        }

        if (!Enum.TryParse<SoftwareReleaseStatus>(dto.Status, out var status))
        {
            return BadRequest("Invalid status");
        }

        var release = new SoftwareRelease
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Version = dto.Version,
            ReleaseDate = dto.ReleaseDate ?? DateTime.UtcNow,
            Status = status,
            ChangelogEn = dto.ChangelogEn,
            ChangelogAr = dto.ChangelogAr,
            RequirementsEn = dto.RequirementsEn,
            RequirementsAr = dto.RequirementsAr,
            IsActive = dto.IsActive
        };

        _context.Set<SoftwareRelease>().Add(release);
        await _context.SaveChangesAsync(CancellationToken.None);

        return CreatedAtAction(nameof(GetReleases), new { productId }, new SoftwareReleaseDto
        {
            Id = release.Id,
            Version = release.Version,
            ReleaseDate = release.ReleaseDate,
            Status = release.Status.ToString(),
            ChangelogEn = release.ChangelogEn,
            ChangelogAr = release.ChangelogAr,
            RequirementsEn = release.RequirementsEn,
            RequirementsAr = release.RequirementsAr,
            IsActive = release.IsActive,
            Files = new List<SoftwareFileDto>()
        });
    }

    /// <summary>
    /// Update a release
    /// </summary>
    [HttpPut("admin/releases/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<SoftwareReleaseDto>> UpdateRelease(Guid id, [FromBody] CreateSoftwareReleaseDto dto)
    {
        var release = await _context.Set<SoftwareRelease>()
            .Include(r => r.Files)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (release == null)
        {
            return NotFound();
        }

        if (!Enum.TryParse<SoftwareReleaseStatus>(dto.Status, out var status))
        {
            return BadRequest("Invalid status");
        }

        release.Version = dto.Version;
        release.ReleaseDate = dto.ReleaseDate ?? release.ReleaseDate;
        release.Status = status;
        release.ChangelogEn = dto.ChangelogEn;
        release.ChangelogAr = dto.ChangelogAr;
        release.RequirementsEn = dto.RequirementsEn;
        release.RequirementsAr = dto.RequirementsAr;
        release.IsActive = dto.IsActive;

        await _context.SaveChangesAsync(CancellationToken.None);

        return Ok(new SoftwareReleaseDto
        {
            Id = release.Id,
            Version = release.Version,
            ReleaseDate = release.ReleaseDate,
            Status = release.Status.ToString(),
            ChangelogEn = release.ChangelogEn,
            ChangelogAr = release.ChangelogAr,
            RequirementsEn = release.RequirementsEn,
            RequirementsAr = release.RequirementsAr,
            IsActive = release.IsActive,
            Files = release.Files.Select(f => new SoftwareFileDto
            {
                Id = f.Id,
                Os = f.Os.ToString(),
                Arch = f.Arch.ToString(),
                FileType = f.FileType.ToString(),
                FileName = f.FileName,
                FileSizeBytes = f.FileSizeBytes,
                Sha256 = f.Sha256,
                DownloadCount = f.DownloadCount,
                IsActive = f.IsActive
            }).ToList()
        });
    }

    /// <summary>
    /// Delete a release
    /// </summary>
    [HttpDelete("admin/releases/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteRelease(Guid id)
    {
        var release = await _context.Set<SoftwareRelease>()
            .Include(r => r.Files)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (release == null)
        {
            return NotFound();
        }

        // Delete files from storage
        foreach (var file in release.Files)
        {
            await _fileStorageService.DeleteFileAsync(file.StoredPath);
        }

        _context.Set<SoftwareRelease>().Remove(release);
        await _context.SaveChangesAsync(CancellationToken.None);

        return NoContent();
    }

    /// <summary>
    /// Upload a file to a release
    /// </summary>
    [HttpPost("admin/releases/{releaseId}/files")]
    [Authorize(Roles = "Admin,Manager")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<SoftwareFileDto>> UploadFile(
        Guid releaseId,
        IFormFile file,
        [FromForm] string os,
        [FromForm] string arch,
        [FromForm] string fileType)
    {
        var release = await _context.Set<SoftwareRelease>()
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.Id == releaseId);

        if (release == null)
        {
            return NotFound("Release not found");
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        if (!Enum.TryParse<SoftwareFileOs>(os, true, out var osEnum))
        {
            return BadRequest("Invalid OS");
        }

        if (!Enum.TryParse<SoftwareFileArch>(arch, true, out var archEnum))
        {
            return BadRequest("Invalid architecture");
        }

        if (!Enum.TryParse<SoftwareFileType>(fileType, true, out var fileTypeEnum))
        {
            return BadRequest("Invalid file type");
        }

        // Calculate SHA-256
        string sha256;
        using (var sha = SHA256.Create())
        {
            using var stream = file.OpenReadStream();
            var hash = await sha.ComputeHashAsync(stream);
            sha256 = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        // Save file
        var storedPath = $"software/{release.Product.Slug}/{release.Version}/{file.FileName}";
        await using (var stream = file.OpenReadStream())
        {
            storedPath = await _fileStorageService.SaveFileAsync(stream, file.FileName, file.ContentType, $"software/{release.Product.Slug}/{release.Version}");
        }

        var softwareFile = new SoftwareFile
        {
            Id = Guid.NewGuid(),
            ReleaseId = releaseId,
            Os = osEnum,
            Arch = archEnum,
            FileType = fileTypeEnum,
            FileName = file.FileName,
            StoredPath = storedPath,
            FileSizeBytes = file.Length,
            Sha256 = sha256,
            IsActive = true
        };

        _context.Set<SoftwareFile>().Add(softwareFile);
        await _context.SaveChangesAsync(CancellationToken.None);

        return CreatedAtAction(nameof(GetReleases), new { productId = release.ProductId }, new SoftwareFileDto
        {
            Id = softwareFile.Id,
            Os = softwareFile.Os.ToString(),
            Arch = softwareFile.Arch.ToString(),
            FileType = softwareFile.FileType.ToString(),
            FileName = softwareFile.FileName,
            FileSizeBytes = softwareFile.FileSizeBytes,
            Sha256 = softwareFile.Sha256,
            DownloadCount = 0,
            IsActive = true
        });
    }

    /// <summary>
    /// Delete a file
    /// </summary>
    [HttpDelete("admin/files/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteFile(Guid id)
    {
        var file = await _context.Set<SoftwareFile>().FindAsync(id);
        if (file == null)
        {
            return NotFound();
        }

        await _fileStorageService.DeleteFileAsync(file.StoredPath);

        _context.Set<SoftwareFile>().Remove(file);
        await _context.SaveChangesAsync(CancellationToken.None);

        return NoContent();
    }

    #endregion
}

#region DTOs

public class SoftwareProductListDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? SummaryEn { get; set; }
    public string? SummaryAr { get; set; }
    public string? Category { get; set; }
    public string? IconKey { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public int ReleasesCount { get; set; }
    public string? LatestVersion { get; set; }
    public DateTime? LatestReleaseDate { get; set; }
    public string? LatestReleaseStatus { get; set; }
    public int DownloadCount { get; set; }
}

public class SoftwareProductDetailDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? SummaryEn { get; set; }
    public string? SummaryAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? Category { get; set; }
    public string? IconKey { get; set; }
    public string? LicenseEn { get; set; }
    public string? LicenseAr { get; set; }
    public List<SoftwareReleaseDto> Releases { get; set; } = new();
}

public class SoftwareReleaseDto
{
    public Guid Id { get; set; }
    public string Version { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ChangelogEn { get; set; }
    public string? ChangelogAr { get; set; }
    public string? RequirementsEn { get; set; }
    public string? RequirementsAr { get; set; }
    public bool IsActive { get; set; } = true;
    public List<SoftwareFileDto> Files { get; set; } = new();
}

public class SoftwareFileDto
{
    public Guid Id { get; set; }
    public string Os { get; set; } = string.Empty;
    public string Arch { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string? Sha256 { get; set; }
    public int DownloadCount { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CreateSoftwareProductDto
{
    public string Slug { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? SummaryEn { get; set; }
    public string? SummaryAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? Category { get; set; }
    public string? IconKey { get; set; }
    public string? LicenseEn { get; set; }
    public string? LicenseAr { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}

public class CreateSoftwareReleaseDto
{
    public string Version { get; set; } = string.Empty;
    public DateTime? ReleaseDate { get; set; }
    public string Status { get; set; } = "Stable";
    public string? ChangelogEn { get; set; }
    public string? ChangelogAr { get; set; }
    public string? RequirementsEn { get; set; }
    public string? RequirementsAr { get; set; }
    public bool IsActive { get; set; } = true;
}

#endregion
