using MediatR;
using Maba.Application.Features.Cms.DTOs;

namespace Maba.Application.Features.Cms.PageSections.Commands;

public class CreatePageSectionDraftCommand : IRequest<PageSectionDraftDto>
{
    public Guid PageId { get; set; }
    public Guid PageSectionTypeId { get; set; }
    public Guid LayoutTypeId { get; set; }
    public string? TitleEn { get; set; }
    public string? TitleAr { get; set; }
    public string? SubtitleEn { get; set; }
    public string? SubtitleAr { get; set; }
    public string? ConfigJson { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? CreatedByUserId { get; set; }
}

