using MediatR;
using Maba.Application.Features.Cms.DTOs;

namespace Maba.Application.Features.Cms.Pages.Commands;

public class CreatePageCommand : IRequest<PageDto>
{
    public string Key { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? TitleEn { get; set; }
    public string? TitleAr { get; set; }
    public string? MetaTitleEn { get; set; }
    public string? MetaTitleAr { get; set; }
    public string? MetaDescriptionEn { get; set; }
    public string? MetaDescriptionAr { get; set; }
    public bool IsHome { get; set; }
    public bool IsActive { get; set; } = true;
}

