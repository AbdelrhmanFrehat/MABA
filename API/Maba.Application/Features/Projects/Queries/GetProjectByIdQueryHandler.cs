using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Projects.DTOs;
using Maba.Domain.Projects;

namespace Maba.Application.Features.Projects.Queries;

public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, ProjectDto?>
{
    private readonly IApplicationDbContext _context;

    public GetProjectByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectDto?> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var project = await _context.Set<Project>()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (project == null) return null;

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
            TechStack = string.IsNullOrEmpty(project.TechStackJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(project.TechStackJson) ?? new List<string>(),
            Highlights = string.IsNullOrEmpty(project.HighlightsJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(project.HighlightsJson) ?? new List<string>(),
            Gallery = string.IsNullOrEmpty(project.GalleryJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(project.GalleryJson) ?? new List<string>(),
            IsFeatured = project.IsFeatured,
            IsActive = project.IsActive,
            SortOrder = project.SortOrder,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt
        };
    }
}
