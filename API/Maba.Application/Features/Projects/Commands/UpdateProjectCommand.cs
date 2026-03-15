using MediatR;
using Maba.Domain.Projects;

namespace Maba.Application.Features.Projects.Commands;

public class UpdateProjectCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public string? TitleEn { get; set; }
    public string? TitleAr { get; set; }
    public string? Slug { get; set; }
    public string? SummaryEn { get; set; }
    public string? SummaryAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public string? CoverImageUrl { get; set; }
    public ProjectCategory? Category { get; set; }
    public ProjectStatus? Status { get; set; }
    public int? Year { get; set; }
    public List<string>? TechStack { get; set; }
    public List<string>? Highlights { get; set; }
    public List<string>? Gallery { get; set; }
    public bool? IsFeatured { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
}
