using MediatR;
using Maba.Application.Features.Common.EmailTemplates.DTOs;

namespace Maba.Application.Features.Common.EmailTemplates.Queries;

public class GetEmailTemplatesQuery : IRequest<List<EmailTemplateDto>>
{
    public bool? IsActive { get; set; }
}

