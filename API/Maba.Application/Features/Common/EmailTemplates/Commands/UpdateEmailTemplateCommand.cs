using MediatR;
using Maba.Application.Features.Common.EmailTemplates.DTOs;

namespace Maba.Application.Features.Common.EmailTemplates.Commands;

public class UpdateEmailTemplateCommand : IRequest<EmailTemplateDto>
{
    public Guid Id { get; set; }
    public string? SubjectEn { get; set; }
    public string? SubjectAr { get; set; }
    public string? BodyHtmlEn { get; set; }
    public string? BodyHtmlAr { get; set; }
    public string? BodyTextEn { get; set; }
    public string? BodyTextAr { get; set; }
    public string? Variables { get; set; }
    public bool? IsActive { get; set; }
}

