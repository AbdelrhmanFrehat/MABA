using MediatR;
using Maba.Application.Features.Common.EmailTemplates.DTOs;

namespace Maba.Application.Features.Common.EmailTemplates.Commands;

public class CreateEmailTemplateCommand : IRequest<EmailTemplateDto>
{
    public string Key { get; set; } = string.Empty;
    public string SubjectEn { get; set; } = string.Empty;
    public string SubjectAr { get; set; } = string.Empty;
    public string BodyHtmlEn { get; set; } = string.Empty;
    public string BodyHtmlAr { get; set; } = string.Empty;
    public string? BodyTextEn { get; set; }
    public string? BodyTextAr { get; set; }
    public string? Variables { get; set; }
    public bool IsActive { get; set; } = true;
}

