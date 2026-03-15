using MediatR;
using Maba.Application.Features.Common.EmailTemplates.DTOs;

namespace Maba.Application.Features.Common.EmailTemplates.Queries;

public class GetEmailTemplateByKeyQuery : IRequest<EmailTemplateDto>
{
    public string Key { get; set; } = string.Empty;
}

