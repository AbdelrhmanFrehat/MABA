using MediatR;
using System.Text.Json;
using System.Text.RegularExpressions;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Projects.DTOs;
using Maba.Domain.Projects;

namespace Maba.Application.Features.Projects.Commands;

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, ProjectDto>
{
    private readonly IApplicationDbContext _context;

    public CreateProjectCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectDto> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var slug = string.IsNullOrWhiteSpace(request.Slug) 
            ? GenerateSlug(request.TitleEn) 
            : request.Slug;

        var project = new Project
        {
            Id = Guid.NewGuid(),
            TitleEn = request.TitleEn,
            TitleAr = request.TitleAr,
            Slug = slug,
            SummaryEn = request.SummaryEn,
            SummaryAr = request.SummaryAr,
            DescriptionEn = request.DescriptionEn,
            DescriptionAr = request.DescriptionAr,
            CoverImageUrl = request.CoverImageUrl,
            Category = request.Category,
            Status = request.Status,
            Year = request.Year,
            TechStackJson = request.TechStack != null ? JsonSerializer.Serialize(request.TechStack) : null,
            HighlightsJson = request.Highlights != null ? JsonSerializer.Serialize(request.Highlights) : null,
            GalleryJson = request.Gallery != null ? JsonSerializer.Serialize(request.Gallery) : null,
            IsFeatured = request.IsFeatured,
            IsActive = request.IsActive,
            SortOrder = request.SortOrder,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<Project>().Add(project);
        await _context.SaveChangesAsync(cancellationToken);

        return new ProjectDto
        {
            Id = project.Id,
            TitleEn = project.TitleEn,
            TitleAr = project.TitleAr,
            Slug = project.Slug,
            SummaryEn = project.SummaryEn,
            SummaryAr = project.SummaryAr,
            DescriptionEn = project.DescriptionEn,
            DescriptionAr = project.DescriptionAr,
            CoverImageUrl = project.CoverImageUrl,
            Category = project.Category,
            Status = project.Status,
            Year = project.Year,
            TechStack = request.TechStack ?? new List<string>(),
            Highlights = request.Highlights ?? new List<string>(),
            Gallery = request.Gallery ?? new List<string>(),
            IsFeatured = project.IsFeatured,
            IsActive = project.IsActive,
            SortOrder = project.SortOrder,
            CreatedAt = project.CreatedAt
        };
    }

    private static string GenerateSlug(string title)
    {
        var slug = title.ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');
        return slug + "-" + Guid.NewGuid().ToString("N")[..6];
    }
}
