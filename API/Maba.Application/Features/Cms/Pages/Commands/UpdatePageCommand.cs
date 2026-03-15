using MediatR;
using Maba.Application.Features.Cms.DTOs;

namespace Maba.Application.Features.Cms.Pages.Commands;

public class UpdatePageCommand : IRequest<PageDto>
{
    public Guid Id { get; set; }
    public string? Key { get; set; }
    public string? Path { get; set; }
    public string? TitleEn { get; set; }
    public string? TitleAr { get; set; }
    public string? MetaTitleEn { get; set; }
    public string? MetaTitleAr { get; set; }
    public string? MetaDescriptionEn { get; set; }
    public string? MetaDescriptionAr { get; set; }
    public bool? IsHome { get; set; }
    public bool? IsActive { get; set; }
    public string? TemplateKey { get; set; }
}

