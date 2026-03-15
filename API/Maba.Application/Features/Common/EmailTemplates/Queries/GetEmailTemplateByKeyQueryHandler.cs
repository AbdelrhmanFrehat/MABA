using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Common.EmailTemplates.Queries;
using Maba.Application.Features.Common.EmailTemplates.DTOs;
using Maba.Domain.Common;

namespace Maba.Application.Features.Common.EmailTemplates.Handlers;

public class GetEmailTemplateByKeyQueryHandler : IRequestHandler<GetEmailTemplateByKeyQuery, EmailTemplateDto>
{
    private readonly IApplicationDbContext _context;

    public GetEmailTemplateByKeyQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmailTemplateDto> Handle(GetEmailTemplateByKeyQuery request, CancellationToken cancellationToken)
    {
        var template = await _context.Set<EmailTemplate>()
            .FirstOrDefaultAsync(t => t.Key == request.Key, cancellationToken);

        if (template == null)
        {
            throw new KeyNotFoundException($"Email template with key '{request.Key}' not found.");
        }

        return new EmailTemplateDto
        {
            Id = template.Id,
            Key = template.Key,
            SubjectEn = template.SubjectEn,
            SubjectAr = template.SubjectAr,
            BodyHtmlEn = template.BodyHtmlEn,
            BodyHtmlAr = template.BodyHtmlAr,
            BodyTextEn = template.BodyTextEn,
            BodyTextAr = template.BodyTextAr,
            Variables = template.Variables,
            IsActive = template.IsActive,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt
        };
    }
}

