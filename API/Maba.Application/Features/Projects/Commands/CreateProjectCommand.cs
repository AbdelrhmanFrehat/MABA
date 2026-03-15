using MediatR;
using Maba.Application.Features.Projects.DTOs;
using Maba.Domain.Projects;

namespace Maba.Application.Features.Projects.Commands;

public class CreateProjectCommand : IRequest<ProjectDto>
{
    public string TitleEn { get; set; } = string.Empty;
    public string? TitleAr { get; set; }
    public string? Slug { get; set; }
    public string? SummaryEn { get; set; }
    public string? SummaryAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? CoverImageUrl { get; set; }
    public ProjectCategory Category { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Draft;
    public int Year { get; set; }
    public List<string>? TechStack { get; set; }
    public List<string>? Highlights { get; set; }
    public List<string>? Gallery { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
