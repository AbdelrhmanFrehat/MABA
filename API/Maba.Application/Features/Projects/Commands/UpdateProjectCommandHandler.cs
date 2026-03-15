using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Projects;

namespace Maba.Application.Features.Projects.Commands;

public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateProjectCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _context.Set<Project>()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (project == null) return false;

        if (request.TitleEn != null) project.TitleEn = request.TitleEn;
        if (request.TitleAr != null) project.TitleAr = request.TitleAr;
        if (request.Slug != null) project.Slug = request.Slug;
        if (request.SummaryEn != null) project.SummaryEn = request.SummaryEn;
        if (request.SummaryAr != null) project.SummaryAr = request.SummaryAr;
        if (request.DescriptionEn != null) project.DescriptionEn = request.DescriptionEn;
        if (request.DescriptionAr != null) project.DescriptionAr = request.DescriptionAr;
        if (request.CoverImageUrl != null) project.CoverImageUrl = request.CoverImageUrl;
        if (request.Category.HasValue) project.Category = request.Category.Value;
        if (request.Status.HasValue) project.Status = request.Status.Value;
        if (request.Year.HasValue) project.Year = request.Year.Value;
        if (request.TechStack != null) project.TechStackJson = JsonSerializer.Serialize(request.TechStack);
        if (request.Highlights != null) project.HighlightsJson = JsonSerializer.Serialize(request.Highlights);
        if (request.Gallery != null) project.GalleryJson = JsonSerializer.Serialize(request.Gallery);
        if (request.IsFeatured.HasValue) project.IsFeatured = request.IsFeatured.Value;
        if (request.IsActive.HasValue) project.IsActive = request.IsActive.Value;
        if (request.SortOrder.HasValue) project.SortOrder = request.SortOrder.Value;

        project.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
